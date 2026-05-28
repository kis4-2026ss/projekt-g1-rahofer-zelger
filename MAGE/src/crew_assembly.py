import os
import time
import json
import re
from pathlib import Path

from crewai.agents import AgentAction
from dotenv import load_dotenv
from typing import Dict, List, Annotated, Any
from operator import add
from typing_extensions import TypedDict

# CrewAI & LangGraph Imports
from crewai import Agent, Task, LLM
from crewai_tools import FileReadTool
from crewai.agents.parser import AgentAction, AgentFinish
from crewai.tools import tool
from langchain_community.tools import ShellTool
from langgraph.graph import StateGraph, END
from crewai.tools import BaseTool

# --- 1. Path, Environment & Global Config Setup ---
PROJECT_ROOT = Path(__file__).resolve().parent.parent
load_dotenv(PROJECT_ROOT / ".env")

# Global Iteration Limit for QA -> Developer loop
MAX_QA_DEV_ITERATIONS = 500000

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

# --- 2. LLM Configuration (Optimized for Local Ollama Stability) ---
llm_config = LLM(
    model="ollama/qwen3.5-crew:latest",
    base_url="http://localhost:11434",
    extra_body={
        "options": {
            "num_ctx": 16384,
            "num_predict": 4096,
            "stop": ["Observation:", "<|im_end|>", "###"]
        }
    }
)

# --- 3. Custom Tools ---
base_shell = ShellTool()


class GitTool(BaseTool):
    name: str = "git_tool"
    description: str = (
        "Manage repository history. "
        "Provide raw Git subcommands without the leading 'git'. "
        "Usage: git_tool._run('commit -m \"feat: msg\"') or 'add . && commit -m \"...\"'"
    )

    def _run(self, command: str) -> str:
        if any(forbidden in command for forbidden in ["push", "pull", "remote"]):
            return "Error: Remote operations are disabled for security."

        if command.lstrip().startswith("git "):
            command = command.lstrip()[4:].lstrip()

        return base_shell.run(f"git -C {SRC_PATH} {command}")


class DotNetTool(BaseTool):
    name: str = "dotnet_tool"
    description: str = "C# lifecycle management (build, test, run). Provide raw dotnet subcommands (e.g., 'build', 'run', 'test'). Do NOT include 'dotnet' prefix unless necessary."

    def _run(self, command: str) -> str:
        command = command.strip()
        if not command:
            return "Error: Empty command"

        dangerous = ["nuget delete", "workload install", "tool install --global"]
        if any(d in command for d in dangerous):
            return "Error: Dangerous dotnet operations are disabled."

        full_cmd = command if command.startswith("dotnet") else f"dotnet {command}"
        return base_shell.run(full_cmd, cwd=SRC_PATH)


class SafeFileReadTool(FileReadTool):
    def _run(self, file_path: str, **kwargs) -> str:
        full_path = (ALLOWED_BASE / file_path).resolve()
        if not str(full_path).startswith(str(ALLOWED_BASE)):
            return f"Error: Access denied – path outside agent_workspace: {file_path}"
        if not full_path.exists():
            return f"Error: File not found: {file_path}"
        return super()._run(str(full_path), **kwargs)


@tool("SafeFileWriter")
def safe_file_writer(file_path: str, content: str) -> str:
    """Writes content to a file, but only if the path is within agent_workspace."""
    full_path = (ALLOWED_BASE / file_path).resolve()
    if not str(full_path).startswith(str(ALLOWED_BASE)):
        return f"Error: Access denied – cannot write outside agent_workspace: {file_path}"
    full_path.parent.mkdir(parents=True, exist_ok=True)
    try:
        with open(full_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return f"Successfully wrote to {full_path}"
    except Exception as e:
        return f"Error writing file: {e}"


@tool("SafeFileRead")
def safe_file_read(file_path: str) -> str:
    """Reads a file, but only if the path is within agent_workspace."""
    full_path = (ALLOWED_BASE / file_path).resolve()
    if not str(full_path).startswith(str(ALLOWED_BASE)):
        return f"Error: Access denied – path outside agent_workspace: {file_path}"
    if not full_path.exists():
        return f"Error: File not found: {file_path}"
    try:
        with open(full_path, 'r', encoding='utf-8') as f:
            return f.read()
    except Exception as e:
        return f"Error reading file: {e}"


@tool("SafeDirectoryRead")
def safe_directory_read(directory_path: str = ".") -> str:
    """Lists directory contents, but only if the path is within agent_workspace."""
    full_path = (ALLOWED_BASE / directory_path).resolve()
    if not str(full_path).startswith(str(ALLOWED_BASE)):
        return f"Error: Access denied – directory outside agent_workspace: {directory_path}"
    if not full_path.is_dir():
        return f"Error: Not a directory: {directory_path}"
    try:
        items = '\n'.join(str(p.relative_to(ALLOWED_BASE)) for p in full_path.iterdir())
        return f"Contents of {directory_path}:\n{items}"
    except Exception as e:
        return f"Error reading directory: {e}"


@tool("SafeFileRemove")
def safe_file_remove(file_path: str) -> str:
    """Deletes a file, but only if the path is within agent_workspace."""
    full_path = (ALLOWED_BASE / file_path).resolve()
    if not str(full_path).startswith(str(ALLOWED_BASE)):
        return f"Error: Access denied – cannot delete outside agent_workspace: {file_path}"
    if not full_path.exists():
        return f"Error: File not found: {file_path}"
    if full_path.is_dir():
        return f"Error: Path is a directory, not a file: {file_path}"
    try:
        full_path.unlink()
        return f"Successfully deleted: {full_path}"
    except Exception as e:
        return f"Error deleting file: {e}"


git_tool = GitTool()
dotnet_tool = DotNetTool()
file_tools = [safe_file_read, safe_file_writer, safe_directory_read, safe_file_remove]


# --- 4. State Definition ---
class ScrumState(TypedDict):
    next_node: str
    role_violation_flag: bool
    messages: Annotated[list, add]
    qa_dev_iterations: int

    # artifacts
    product_backlog: List[Dict[str, str]]
    sprint_backlog: List[Dict[str, str]]
    current_increment: Dict[str, str]
    qa_results: Dict[str, Any]


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
    5. NO CODE. NO XAML."""
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
    2. REJECT if: Missing READMEs, missing Conventional Commits, or PO wrote source code."""
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
    1. Write C# (dotnet_tool is available with .NET 8.0) and Avalonia XAML only.
    2. MANDATORY: Every subsystem folder needs its own README.md.
    3. Use safe_directory_read before every modification to maintain context.
    4. Implement math: T = (Recipe Output / Crafting Time) * Machine Speed.
    5. The factorio_recipes_and_machines.json is your single source of truth when it comes to ingame recipes and machines.
    6. Commit after every feature, use the tools git_tool."""
)

qa_tester = Agent(
    role="QA Tester",
    goal="Verify math accuracy and system integrity via xUnit tests.",
    backstory="You verify throughput accuracy and ensure documentation matches reality.",
    llm=llm_config,
    tools=[dotnet_tool] + file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
    1. Write C# test projects based on Gherkin specs.
    2. You MUST use the `dotnet_tool` to run the tests. Do not guess the results.
    3. Your final output MUST be a valid JSON block containing exactly these two keys:
       - "tests_executed": boolean (true if you actually ran dotnet test, false otherwise)
       - "all_passed": boolean (true ONLY if the dotnet_tool reported 0 failures)
    Do not include any text after the JSON block."""
)


# --- 6. Node Logic ---
def execute_with_retry(agent, task_desc, max_retries=10):
    """Execute a task with retry on empty response."""
    for attempt in range(max_retries):
        task = Task(description=task_desc, agent=agent, expected_output="Defined artifact.")
        res_obj = task.execute_sync()
        result_str = str(res_obj)
        if result_str and len(result_str.strip()) > 0:
            return result_str
        print(f"Empty response, retry {attempt + 1}/{max_retries}")
        time.sleep(2)
    return ""


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

    if latest_specs and not latest_code:
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
    latest_message = state["messages"][-1] if state["messages"] else ""
    qa_report = state.get("qa_results", {}).get("report", "")

    feedback_context = ""
    if state.get("role_violation_flag"):
        feedback_context = f"\nYOUR PREVIOUS CODE WAS REJECTED BY THE SCRUM MASTER. Fix it based on this feedback:\n{latest_message}"
    elif state.get("qa_results", {}).get("passed") is False:
        feedback_context = f"\nYOUR PREVIOUS CODE FAILED QA TESTING. Fix the bugs detailed in this report:\n{qa_report}"

    prompt = f"Review {SRC_PATH}. Implement specs: {specs}. \n{feedback_context}\nUpdate Subsystem READMEs and Commit via Git."
    output = execute_with_retry(developer, prompt)

    current_inc = state.get("current_increment", {})
    current_inc["code"] = output

    return {
        "messages": ["DA: Work complete."],
        "current_increment": current_inc,
        "next_node": "scrum_master"
    }


def qa_tester_node(state: ScrumState) -> Dict:
    specs = state.get("current_increment", {}).get("specs", "")
    iterations = state.get("qa_dev_iterations", 0)

    prompt = f"Test the implementation in {SRC_PATH} against specs: {specs}."
    output = execute_with_retry(qa_tester, prompt)

    passed = False
    json_match = re.search(r'\{.*\}', output.replace('\n', ''))
    if json_match:
        try:
            result_data = json.loads(json_match.group(0))
            passed = result_data.get("tests_executed", False) and result_data.get("all_passed", False)
        except json.JSONDecodeError:
            passed = False
            output += "\n[SYSTEM: Failed to parse QA output as JSON.]"

    qa_results = {
        "passed": passed,
        "report": output
    }

    if passed:
        next_node = "end"
        msg = "QA Result: Passed. Finishing process."
    else:
        if iterations >= MAX_QA_DEV_ITERATIONS:
            next_node = "end"
            msg = f"QA Result: Failed - Max QA/Dev iterations ({MAX_QA_DEV_ITERATIONS}) reached."
        else:
            next_node = "developer"
            msg = f"QA Result: Failed - Re-routing back to Developer (Iteration {iterations + 1}/{MAX_QA_DEV_ITERATIONS})."
            iterations += 1

    return {
        "messages": [msg],
        "qa_results": qa_results,
        "qa_dev_iterations": iterations,
        "next_node": next_node
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

    # Run direct sync without wrapping it in an explicit asyncio context block
    main_loop(initial_setup)
