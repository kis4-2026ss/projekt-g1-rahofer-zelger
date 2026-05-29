import os
import time
import shlex
import subprocess
import shutil
from pathlib import Path

from dotenv import load_dotenv
from typing import Dict, List, Annotated, Any
from operator import add
from typing_extensions import TypedDict

# CrewAI & LangGraph Imports
from crewai import Agent, Task, LLM
from crewai.tools import tool
from langgraph.graph import StateGraph, END

# --- 1. Path, Environment & Global Config Setup ---
PROJECT_ROOT = Path(__file__).resolve().parent.parent
load_dotenv(PROJECT_ROOT / ".env")

# Global Iteration Limit for QA -> Developer loop (reduced from 500k)
MAX_QA_DEV_ITERATIONS = 10

# Resolve workspace paths from environment or defaults
raw_issues_path = os.getenv("AGENT_ISSUES_PATH", "./agent_workspace/issues")
ISSUES_PATH = str((PROJECT_ROOT / raw_issues_path).resolve())

raw_src_path = os.getenv("AGENT_SRC_PATH", "./agent_workspace/src")
SRC_PATH = str((PROJECT_ROOT / raw_src_path).resolve())
ALLOWED_BASE = (PROJECT_ROOT / "agent_workspace").resolve()

# Environment settings for tool safety
os.environ["CREWAI_TOOLS_ALLOW_UNSAFE_PATHS"] = "true"

# Ensure directories exist
Path(ISSUES_PATH).mkdir(parents=True, exist_ok=True)
Path(SRC_PATH).mkdir(parents=True, exist_ok=True)

# --- 2. LLM Configuration (using stable Qwen model) ---
llm_config = LLM(
    model="ollama/omnicoder-crew:latest",
    base_url="http://localhost:11434",
    timeout=600,
    extra_body={
        "options": {
            "num_ctx": 16384,
            "num_predict": 4096,
            "stop": ["Observation:", "<|im_end|>"]
        }
    }
)

# --- 3. Custom Tools ---
def _resolve_workspace_path(input_path: str) -> Path:
    """Resolve a user supplied path and ensure it stays inside agent_workspace."""
    # Normalize: remove leading '/app/agent_workspace/' or 'app/agent_workspace/'
    normalized = input_path.lstrip('/')
    if normalized.startswith('app/agent_workspace/'):
        normalized = normalized[len('app/agent_workspace/'):]
    elif normalized.startswith('agent_workspace/'):
        normalized = normalized[len('agent_workspace/'):]

    candidate = Path(normalized)
    if candidate.is_absolute():
        full_path = candidate.resolve()
    else:
        full_path = (ALLOWED_BASE / candidate).resolve()

    try:
        full_path.relative_to(ALLOWED_BASE)
    except ValueError as exc:
        raise ValueError(f"path outside agent_workspace: {input_path}") from exc

    return full_path

def _run_command(args: List[str], cwd: Path, timeout: int = 120) -> str:
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
            check=False
        )
    except subprocess.TimeoutExpired:
        return f"Error: command timed out after {timeout} seconds: {shlex.join(args)}"
    except FileNotFoundError:
        return f"Error: command not found: {args[0]}"
    except Exception as e:
        return f"Error executing command: {e}"

    output = result.stdout + result.stderr
    if result.returncode != 0:
        return f"Command failed with exit code {result.returncode}:\n{output}"
    return output


def _command_failed(output: str) -> bool:
    return output.startswith("Command failed") or output.startswith("Error:")


# --- Git Tool ---
@tool("GitTool")
def git_tool(command: str) -> str:
    """
    Manage repository history and version control.
    Usage: git_tool(command="commit -m 'feat: add logic'") or git_tool(command="add .")
    Note: Do not include the leading 'git' prefix. Remote operations (push/pull) are disabled.
    """
    try:
        args = shlex.split(command)
    except ValueError as e:
        return f"Error: Invalid git command: {e}"

    if args[:1] == ["git"]:
        args = args[1:]
    if not args:
        return "Error: Empty git command"

    if args[0] in {"push", "pull", "remote", "fetch"}:
        return "Error: Remote operations are disabled for security."
    if "-C" in args or any(arg.startswith(("--git-dir", "--work-tree")) for arg in args):
        return "Error: Changing git repository paths is disabled."

    return _run_command(["git", *args], Path(SRC_PATH), timeout=60)


# --- .NET Tool (improved with timeout and direct subprocess) ---
@tool("DotNetTool")
def dotnet_tool(command: str) -> str:
    """
    C# lifecycle management including build, test, and run.
    Usage: dotnet_tool(command="build") or dotnet_tool(command="test")
    Note: Provide raw subcommands. Dangerous operations like 'nuget delete' are disabled.
    """
    try:
        args = shlex.split(command)
    except ValueError as e:
        return f"Error: Invalid dotnet command: {e}"

    if args[:1] == ["dotnet"]:
        args = args[1:]
    if not args:
        return "Error: Empty command"

    dangerous = (
        args[:2] == ["nuget", "delete"]
        or args[:2] == ["workload", "install"]
        or (args[:2] == ["tool", "install"] and "--global" in args)
    )
    if dangerous:
        return "Error: Dangerous dotnet operations are disabled."

    return _run_command(["dotnet", *args], Path(SRC_PATH), timeout=120)


# --- File Management Tools (unchanged) ---
@tool("SafeFileWriter")
def safe_file_writer(file_path: str, content: str) -> str:
    """
    Writes text content to a specific file path within the workspace.
    Usage: safe_file_writer(file_path="Models/User.cs", content="public class User {}")
    """
    try:
        full_path = _resolve_workspace_path(file_path)
    except ValueError as e:
        return f"Error: Access denied – cannot write {e}"

    full_path.parent.mkdir(parents=True, exist_ok=True)
    try:
        with open(full_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return f"Successfully wrote to {file_path}"
    except Exception as e:
        return f"Error writing file: {e}"


@tool("SafeFileRead")
def safe_file_read(file_path: str) -> str:
    """
    Reads the content of a file within the workspace.
    Usage: safe_file_read(file_path="Program.cs")
    """
    try:
        full_path = _resolve_workspace_path(file_path)
    except ValueError as e:
        return f"Error: Access denied – {e}"
    if not full_path.exists():
        return f"Error: File not found: {file_path}"

    try:
        with open(full_path, 'r', encoding='utf-8') as f:
            return f.read()
    except Exception as e:
        return f"Error reading file: {e}"


@tool("SafeDirectoryRead")
def safe_directory_read(directory_path: str = ".") -> str:
    """
    Lists all files and directories within a specific path.
    Usage: safe_directory_read(directory_path="Controllers")
    """
    try:
        full_path = _resolve_workspace_path(directory_path)
    except ValueError as e:
        return f"Error: Access denied – {e}"
    if not full_path.is_dir():
        return f"Error: Not a directory: {directory_path}"

    try:
        items = '\n'.join(str(p.relative_to(ALLOWED_BASE)) for p in sorted(full_path.iterdir()))
        return f"Contents of {directory_path}:\n{items}"
    except Exception as e:
        return f"Error reading directory: {e}"


@tool("SafeFileRemove")
def safe_file_remove(file_path: str) -> str:
    """
    Permanently deletes a file from the workspace.
    Usage: safe_file_remove(file_path="temp_file.txt")
    """
    try:
        full_path = _resolve_workspace_path(file_path)
    except ValueError as e:
        return f"Error: Access denied – cannot delete {e}"
    if not full_path.exists():
        return f"Error: File not found: {file_path}"
    if full_path.is_dir():
        return f"Error: Path is a directory, not a file: {file_path}"

    try:
        full_path.unlink()
        return f"Successfully deleted: {file_path}"
    except Exception as e:
        return f"Error deleting file: {e}"


@tool("SafeFileMove")
def safe_file_move(source_path: str, destination_path: str) -> str:
    """
    Moves or renames a file or directory within the workspace.
    Usage: safe_file_move(source_path="old_name.cs", destination_path="NewName.cs")
    """
    try:
        src_full = _resolve_workspace_path(source_path)
        dest_full = _resolve_workspace_path(destination_path)
    except ValueError as e:
        return f"Error: Access denied – {e}"

    if not src_full.exists():
        return f"Error: Source file not found: {source_path}"
    if dest_full.exists():
        return f"Error: Destination already exists: {destination_path}"

    try:
        # Ensure destination directory exists
        dest_full.parent.mkdir(parents=True, exist_ok=True)

        shutil.move(str(src_full), str(dest_full))
        return f"Successfully moved/renamed {source_path} to {destination_path}"
    except Exception as e:
        return f"Error moving file: {e}"


file_tools = [safe_file_read, safe_file_writer, safe_directory_read, safe_file_remove, safe_file_move]

# --- 4. State Definition ---
class ScrumState(TypedDict):
    next_node: str
    role_violation_flag: bool
    messages: Annotated[list, add]
    qa_dev_iterations: int
    project_structure: str

    # artifacts
    product_backlog: List[Dict[str, str]]
    sprint_backlog: List[Dict[str, str]]
    current_increment: Dict[str, str]
    qa_results: Dict[str, Any]


def get_project_map(root_path: Path) -> str:
    """
    Generates a filtered text-based tree of the workspace.
    Excludes .git, bin/obj, Debug/Release, and hidden files to keep context small.
    """
    tree = []
    for path in sorted(root_path.rglob('*')):
        # Skip entire .git directory
        if '.git' in path.parts:
            continue
        # Skip bin/obj/Debug/Release folders anywhere
        if any(part in ['bin', 'obj', 'Debug', 'Release', 'packages'] for part in path.parts):
            continue
        # Skip hidden files/folders
        if path.name.startswith('.'):
            continue
        depth = len(path.relative_to(root_path).parts)
        spacer = '  ' * (depth - 1)
        tree.append(f"{spacer}- {path.name} {'(DIR)' if path.is_dir() else ''}")
        # Truncate if too large (safety)
        if len(tree) > 500:
            tree.append("  ... (truncated)")
            break
    return "\n".join(tree) if tree else "Workspace is currently empty."


# --- 5. Agent Definitions ---
product_owner = Agent(
    role="Product Owner",
    goal="Define Factorio Modeler requirements and vision using internal recipe knowledge.",
    backstory=f"You define the 'What'. You specify Gherkin stories for items/min math. You maintain the Root README.md in {ISSUES_PATH}.",
    llm=llm_config,
    tools=file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
    1. Before writing anything new, read all available issues and .md files to ensure atomicity.
    2. Write ONLY Gherkin (Given/When/Then).
    3. MANDATORY: Maintain Root README.md in {ISSUES_PATH}.
    4. Ensure math for Advanced Circuits (10/min) matches standard Factorio ratios.
    5. NO CODE. NO XAML.
    6. ATOMICITY: Check existing specs in the workspace structure to avoid duplicating 
       Gherkin stories or requirements.
    {project_structure}
    """
)

scrum_master = Agent(
    role="Scrum Master",
    goal="Audit role boundaries and ensure documentation completeness.",
    backstory="You are the process gatekeeper. You reject work that violates roles or lacks subsystem READMEs.",
    llm=llm_config,
    tools=[git_tool] + file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
        1. Output JSON backlogs and Audit Reports.
        2. REJECT if: Missing READMEs, missing Conventional Commits, or PO wrote code.
        3. CASING AUDIT: Reject work if the developer created duplicate files with 
           different casing or redundant naming. Reference the workspace structure:
        {project_structure}
        """
)

developer = Agent(
    role="Developer",
    goal="Implement the C# Factorio Modeler and document technical architecture.",
    backstory=f"You implement logic in {SRC_PATH}. You document every subsystem with its own README.md. You never overwrite code without reading it first.",
    llm=llm_config,
    tools=[git_tool, dotnet_tool] + file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
        1. Write C# (use dotnet_tool) and Avalonia XAML only.
        2. MANDATORY: Every subsystem folder needs its own README.md.
        3. SINGLETON ARCHITECT: Before creating any file, check the CURRENT WORKSPACE below. 
           If a file exists with different casing (e.g., 'models' vs 'Models'), REUSE the existing one.
        4. Use safe_directory_read before every modification to maintain context.
        5. Implement math: T = (Recipe Output / Crafting Time) * Machine Speed.
        6. Use factorio_recipes_and_machines.json as the single source of truth.
        7. CURRENT WORKSPACE:
        {project_structure} 
        """
)

# Updated QA agent: must write test files to disk, not just return JSON
qa_tester = Agent(
    role="QA Tester",
    goal="Verify math accuracy and system integrity via xUnit tests.",
    backstory="You verify throughput accuracy and ensure documentation matches reality.",
    llm=llm_config,
    tools=[dotnet_tool] + file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
    1. Write C# xUnit test files using safe_file_writer. Place them in the existing test project (e.g., FactorioModeler.Tests/).
    2. Base your tests on the Gherkin specs provided in the task.
    3. After writing the tests, reply exactly with 'TESTS_WRITTEN'.
    """
)


# --- 6. Node Logic ---
def execute_with_retry(agent, task_desc, max_retries=10):
    """Execute a task with retry on empty response. Returns error string if all fail."""
    for attempt in range(max_retries):
        try:
            task = Task(description=task_desc, agent=agent, expected_output="Defined artifact.")
            res_obj = task.execute_sync()
            result_str = str(res_obj)
            if result_str and len(result_str.strip()) > 0:
                return result_str
        except Exception as e:
            print(f"Error on attempt {attempt+1}/{max_retries}: {e}")
        time.sleep(2)
    return "[ERROR] Agent failed to produce a non-empty response."


def product_owner_node(state: ScrumState) -> Dict:
    latest_input = state["messages"][-1] if state["messages"] else ""
    prompt = f"Analyze input: {latest_input}. Update Gherkin specs and Root README in {ISSUES_PATH}."

    output = execute_with_retry(product_owner, prompt)

    current_inc = state.get("current_increment", {})
    current_inc["specs"] = output

    return {
        "messages": [f"POA: {output}"],
        "current_increment": current_inc,
        "next_node": "scrum_master"
    }


def scrum_master_node(state: ScrumState) -> Dict:
    latest_specs = state.get("current_increment", {}).get("specs", "")
    latest_code = state.get("current_increment", {}).get("code", "")

    # Validate non-empty specs before allowing developer
    if latest_specs and not latest_code:
        if not latest_specs.strip() or len(latest_specs.strip()) < 50:
            return {
                "next_node": "product_owner",
                "messages": ["SMA: Empty or insufficient spec – returning to Product Owner."]
            }

        audit_context = f"""
            Audit Specs: {latest_specs}. 
            Verify READMEs and check for role violations. If valid, reply exactly with 'PROCEED'.
        """
        next_destination = "developer"
        fallback_destination = "product_owner"
    else:
        audit_context = f"""
            Audit Code against Specs. 
            SPECS: {latest_specs}
            CODE: {latest_code}
            Check for role violations and missing commits. If valid, reply exactly with 'PROCEED'.
        """
        next_destination = "qa_tester"
        fallback_destination = "developer"

    res = execute_with_retry(scrum_master, audit_context)

    if "PROCEED" in res.upper():
        return {
            "messages": ["SMA: Phase Approved."],
            "next_node": next_destination,
            "role_violation_flag": False
        }
    else:
        return {
            "messages": [f"SMA: Rejected - {res}"],
            "next_node": fallback_destination,
            "role_violation_flag": True
        }


def developer_node(state: ScrumState) -> Dict:
    specs = state.get("current_increment", {}).get("specs", "")
    # Reject empty spec
    if not specs or len(specs.strip()) < 50:
        return {
            "messages": ["DA: Received empty or insufficient spec – routing back to Product Owner"],
            "next_node": "product_owner"
        }

    current_map = get_project_map(ALLOWED_BASE)

    naming_policy = """
    NAMING POLICY:
    - Use PascalCase for all C# files and directories.
    - Check the EXISTING STRUCTURE below. If a folder/file exists with different casing, 
      REUSE it or RENAME it; do not create a duplicate.
    """

    prompt = f"""
    {naming_policy}

    CURRENT WORKSPACE STRUCTURE:
    {current_map}

    TASK: Implement specs: {specs}.
    """

    output = execute_with_retry(developer, prompt)

    return {
        "messages": ["DA: Work complete."],
        "project_structure": current_map,
        "current_increment": {"code": output, "specs": specs},
        "next_node": "scrum_master"
    }


def qa_tester_node(state: ScrumState) -> Dict:
    # Check iteration limit
    iterations = state.get("qa_dev_iterations", 0)
    if iterations >= MAX_QA_DEV_ITERATIONS:
        return {
            "qa_results": {"tests_executed": False, "all_passed": False},
            "next_node": "end",
            "messages": [f"Max iterations ({MAX_QA_DEV_ITERATIONS}) reached – stopping."]
        }

    specs = state.get("current_increment", {}).get("specs", "")
    if not specs or len(specs.strip()) < 50:
        return {
            "qa_results": {"tests_executed": False, "all_passed": False},
            "next_node": "developer",
            "messages": ["QA: No valid specs – cannot write tests."]
        }

    # 1. QA agent writes test files
    prompt = f"Write xUnit tests for these specs:\n{specs}"
    response = execute_with_retry(qa_tester, prompt, max_retries=3)
    if "TESTS_WRITTEN" not in response:
        return {
            "qa_results": {"tests_executed": False, "all_passed": False},
            "next_node": "developer",
            "messages": ["QA: Agent failed to confirm test writing."]
        }

    # 2. Discover solution file (dynamic, not hardcoded)
    sln_files = list(Path(SRC_PATH).glob("*.sln"))
    if not sln_files:
        return {
            "qa_results": {"tests_executed": True, "all_passed": False},
            "next_node": "developer",
            "messages": ["QA: No .sln file found – cannot build."]
        }
    sln_name = sln_files[0].name

    # 3. Build
    build_out = dotnet_tool(f"build {shlex.quote(sln_name)} --no-restore")
    if _command_failed(build_out):
        return {
            "qa_results": {"tests_executed": True, "all_passed": False},
            "next_node": "developer",
            "messages": [f"QA: Build failed:\n{build_out[:300]}"]
        }

    # 4. Test
    test_out = dotnet_tool(f"test {shlex.quote(sln_name)} --no-build --verbosity normal")
    passed = not _command_failed(test_out)

    iterations += 1

    return {
        "qa_results": {"tests_executed": True, "all_passed": passed},
        "qa_dev_iterations": iterations,
        "next_node": "end" if passed else "developer",
        "messages": [f"QA: {'Passed' if passed else 'Failed'} - {test_out[:200]}"]
    }


# --- 7. Graph Assembly ---
builder = StateGraph(ScrumState)
builder.add_node("product_owner", product_owner_node)
builder.add_node("scrum_master", scrum_master_node)
builder.add_node("developer", developer_node)
builder.add_node("qa_tester", qa_tester_node)

builder.set_entry_point("product_owner")


def routing_router(state: ScrumState) -> str:
    return state.get("next_node", "end")


builder.add_conditional_edges("product_owner", routing_router, {"scrum_master": "scrum_master", "end": END})
builder.add_conditional_edges("scrum_master", routing_router, {
    "developer": "developer",
    "qa_tester": "qa_tester",
    "product_owner": "product_owner",
    "end": END
})
builder.add_conditional_edges("developer", routing_router, {"scrum_master": "scrum_master", "end": END})
builder.add_conditional_edges("qa_tester", routing_router,
                              {"developer": "developer", "scrum_master": "scrum_master", "end": END})

scrum_app = builder.compile()


# --- 8. Synchronous Execution Entrypoint ---
def main_loop(initial_state):
    print("--- MAGE-SCRUM RUNTIME STARTED (OFFLINE MODE) ---")
    for event in scrum_app.stream(initial_state):
        for node, update in event.items():
            print(f"\n[NODE]: {node}")
            if "messages" in update:
                print(f"[LOG]: {update['messages'][-1]}")

if __name__ == "__main__":
    initial_setup = {
        "messages": [
            """
            ### SYSTEM INITIATION: FACTORIO ARCHITECT ENGINE

            **CRITICAL ARCHITECTURAL REQUIREMENT:** 
            You are building a production modeling tool. You must maintain structural division of labor.
            - Product Owner: Output ONLY functional specifications, metrics, and Gherkin. No C# syntax.
            - Developer: Output ONLY robust, compilable .NET 8 C# source logic, Avalonia XAML code, and markdown documentation. No loose conversational commentary.
            - QA Tester: Execute automated xUnit tests using the dotnet_tool and return the structural metric JSON.

            ---

            ### 1. CORE SIMULATION DATA & MECHANICS
            - **Data Source File**: `/app/agent_workspace/src/factorio_recipes_and_machines.json`
            - **Mathematical Model**: Production Throughput ($T$) per minute must be calculated strictly using the formula:
              $$T = \\left( \\frac{\\text{Recipe Output Qty}}{\\text{Recipe Crafting Time}} \\right) \\times \\text{Machine Crafting Speed} \\times 60$$
            - **Target Baseline Verification Tasks**:
              1. *Advanced Circuit Production*: Verify a target assembly chain outputting exactly $10/\\text{min}$.
              2. *Express Splitter Production*: Verify a target assembly chain outputting exactly $2.5/\\text{min}$.

            ---

            ### 2. CORE SUBSYSTEM COMPONENT REQUIREMENTS
            - **Backend Engine Core**: Pure C# library managing recipe deserialization, graph nodes, structural relationships, factory lines, and throughput evaluation algorithms.
            - **Graphical User Interface**: Cross-Platform desktop application leveraging `Avalonia.Templates` UI controls. Must include a workflow canvas displaying structural throughput metrics alongside item emojis (🏭, ⚙️, 🟦).
            - **Model Context Protocol (MCP) Server**: Interoperable JSON-RPC API exposing exactly three technical capabilities:
              - `add_node(string itemType, string machineType)`
              - `connect_nodes(string sourceId, string targetId)`
              - `get_bottlenecks()`

            The entire C# should be accessible via a single .sln file.
            ---

            ### 3. AUTOMATION & INFRASTRUCTURE MANDATES
            - **Version Control System**: Run initialization via `git init` directly inside the source directory. Every functional advancement or modification must map to an isolated git state change using standard Conventional Commit formatting (`feat:`, `fix:`, `docs:`, `refactor:`).
            - **Context Isolation Policy**: The Developer must read targeted project workspaces using directory scanning tools prior to changing or creating code structures. Do not rewrite code blindly.
            - **System Documentation Rules**: 
              - The central `/app/agent_workspace/issues/README.md` belongs exclusively to the Product Owner.
              - Every isolated technical C# project subdirectory must contain a dedicated, developer-maintained `README.md` file specifying internal architectures, module dependencies, and compilation guides.
            
            
            ### 4. ACCEPTANCE TESTS (MUST BE VERIFIED BY QA TESTER)

            The QA Tester MUST write and execute xUnit tests that validate the following four acceptance criteria.

            #### Test 1: Advanced Circuit Throughput (10/min)
            ```gherkin
            Feature: Advanced Circuit Production Target
              Scenario: Verify assembly chain outputs exactly 10 advanced circuits per minute
                Given a production chain for advanced circuits using assembling machine 3 (crafting speed = 1.25)
                And the recipe: output=1, crafting_time=6 seconds
                When the total throughput is calculated using T = (output_qty / crafting_time) * machine_speed * 60
                Then the throughput per minute equals exactly 10
            ```
            
            #### Test 2: Express Splitter Throughput (2.5/min)
            ```gherkin
            Feature: Express Splitter Production Target
              Scenario: Verify assembly chain outputs exactly 2.5 express splitters per minute
                Given a production chain for express splitters using assembling machine 3 (crafting speed = 1.25)
                And the recipe: output=1, crafting_time=2 seconds
                When the total throughput is calculated using T = (output_qty / crafting_time) * machine_speed * 60
                Then the throughput per minute equals exactly 2.5
            ```
            
            #### Test 3: MCP Server Exposes Required Tools
            ```gherkin
            Feature: MCP Server Interface
              Scenario: The MCP server must support three specific JSON-RPC methods
                Given the MCP server is running on localhost:5000
                When I send a request for `add_node` with parameters `itemType` and `machineType`
                Then a valid JSON-RPC response with method name `add_node` is returned
                And the same for `connect_nodes` (sourceId, targetId) and `get_bottlenecks` (no parameters)
            ```
            """
        ],
        "next_node": "product_owner",
        "role_violation_flag": False,
        "qa_dev_iterations": 0,
        "product_backlog": [],
        "sprint_backlog": [],
        "current_increment": {
            "specs": "",
            "code": ""
        },
        "qa_results": {}
    }

    main_loop(initial_setup)