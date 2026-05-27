import os
import time
import json
from pathlib import Path
from dotenv import load_dotenv
from typing import Dict, List, Annotated, Any
from operator import add
from typing_extensions import TypedDict

# CrewAI & LangGraph Imports
from crewai import Agent, Task, LLM
from crewai_tools import FileReadTool, FileWriterTool, DirectoryReadTool
from langchain_community.tools import ShellTool
from langgraph.graph import StateGraph, END
from crewai.tools import BaseTool

# --- 1. Path, Environment & Global Config Setup ---
PROJECT_ROOT = Path(__file__).resolve().parent.parent
load_dotenv(PROJECT_ROOT / ".env")

# Global Iteration Limit for QA -> Developer loop
MAX_QA_DEV_ITERATIONS = 2000

# Resolve workspace paths from environment or defaults
raw_issues_path = os.getenv("AGENT_ISSUES_PATH", "./agent_workspace/issues")
ISSUES_PATH = str((PROJECT_ROOT / raw_issues_path).resolve())

raw_src_path = os.getenv("AGENT_SRC_PATH", "./agent_workspace/src")
SRC_PATH = str((PROJECT_ROOT / raw_src_path).resolve())

# Environment settings for tool safety
os.environ["CREWAI_TOOLS_ALLOW_UNSAFE_PATHS"] = "true"

# Ensure directories exist
Path(ISSUES_PATH).mkdir(parents=True, exist_ok=True)
Path(SRC_PATH).mkdir(parents=True, exist_ok=True)

# --- 2. LLM Configuration ---
llm_config = LLM(
    model="ollama/qwen3.5-opencode:latest",
    base_url="http://localhost:11434",
    api_key="NA"
)


# --- 3. Custom Tools & Metrics Logic ---
class MetricsTracker:
    """Aggregates performance data across the graph run."""

    def __init__(self):
        self.total_tokens = 0
        self.total_time = 0.0
        self.calls = 0

    def update(self, result_obj: Any, duration: float):
        self.total_time += duration
        self.calls += 1
        usage = getattr(result_obj, 'token_usage', None)
        if usage:
            self.total_tokens += usage.total_tokens


metrics = MetricsTracker()
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


git = GitTool()
dotnet = DotNetTool()
file_tools = [FileReadTool(), FileWriterTool(), DirectoryReadTool()]


# --- 4. State Definition (Updated with iteration counter) ---
class ScrumState(TypedDict):
    next_node: str
    role_violation_flag: bool
    messages: Annotated[list, add]
    qa_dev_iterations: int  # Tracking iterations for QA -> Dev loop

    # artifacts
    product_backlog: List[Dict[str, str]]
    sprint_backlog: List[Dict[str, str]]
    current_increment: Dict[str, str]
    qa_results: Dict[str, Any]


# --- 5. Agent Definitions (Strict Project Focus from crew_assembly.py) ---
product_owner = Agent(
    role="Product Owner",
    goal="Define Factorio Modeler requirements and vision using internal recipe knowledge.",
    backstory=f"You define the 'What'. You specify Gherkin stories for items/min math. You maintain the Root README.md in {ISSUES_PATH}.",
    llm=llm_config,
    tools=file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
    1. Write ONLY Gherkin (Given/When/Then).
    2. MANDATORY: Maintain Root README.md in {ISSUES_PATH}.
    3. Ensure math for Advanced Circuits (10/min) matches standard Factorio ratios.
    4. NO CODE. NO XAML."""
)

scrum_master = Agent(
    role="Scrum Master",
    goal="Audit role boundaries and ensure documentation completeness.",
    backstory="You are the process gatekeeper. You reject work that violates roles or lacks subsystem READMEs.",
    llm=llm_config,
    tools=[git] + file_tools,
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
    tools=[git, dotnet] + file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
    1. Write C# (.NET 8.0) and Avalonia XAML only.
    2. MANDATORY: Every subsystem folder needs its own README.md.
    3. Use DirectoryReadTool before every modification to maintain context.
    4. Implement math: T = (Recipe Output / Crafting Time) * Machine Speed."""
)

qa_tester = Agent(
    role="QA Tester",
    goal="Verify math accuracy and system integrity via xUnit tests.",
    backstory="You verify throughput accuracy and ensure documentation matches reality.",
    llm=llm_config,
    tools=[dotnet] + file_tools,
    verbose=True,
    allow_delegation=False,
    system_template="""{system_message}
    1. Write C# test projects based on Gherkin specs.
    2. Include 'QA_PASSED' only if all mathematical assertions pass perfectly."""
)


# --- 6. Node Logic (Updated with QA/Dev iteration limits) ---
def execute_with_telemetry(agent, task_desc):
    start = time.time()
    task = Task(description=task_desc, agent=agent, expected_output="Defined artifact.")
    res_obj = task.execute_sync()
    duration = time.time() - start
    metrics.update(res_obj, duration)
    return str(res_obj)


def product_owner_node(state: ScrumState) -> Dict:
    latest_input = state["messages"][-1] if state["messages"] else ""
    prompt = f"Analyze input: {latest_input}. Update Gherkin specs and Root README in {ISSUES_PATH}."

    output = execute_with_telemetry(product_owner, prompt)

    # Initialize current_increment if empty to prevent KeyError downstream
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

    res = execute_with_telemetry(scrum_master, audit_context)

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

    # Inject context from previous node failures
    feedback_context = ""
    if state.get("role_violation_flag"):
        feedback_context = f"\nYOUR PREVIOUS CODE WAS REJECTED BY THE SCRUM MASTER. Fix it based on this feedback:\n{latest_message}"
    elif state.get("qa_results", {}).get("passed") is False:
        feedback_context = f"\nYOUR PREVIOUS CODE FAILED QA TESTING. Fix the bugs detailed in this report:\n{qa_report}"

    prompt = f"Review {SRC_PATH}. Implement specs: {specs}. \n{feedback_context}\nUpdate Subsystem READMEs and Commit via Git."
    output = execute_with_telemetry(developer, prompt)

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
    output = execute_with_telemetry(qa_tester, prompt)

    passed = "QA_PASSED" in output.upper()

    qa_results = {
        "passed": passed,
        "report": output
    }

    if passed:
        next_node = "end"
        msg = "QA Result: Passed. Finishing process."
    else:
        # Check against global iteration limit
        if iterations >= MAX_QA_DEV_ITERATIONS:
            next_node = "end"
            msg = f"QA Result: Failed - Max QA/Dev iterations ({MAX_QA_DEV_ITERATIONS}) reached. Halting process to avoid infinite loop."
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

# --- 8. Main Loop ---
if __name__ == "__main__":
    initial_state = {
        "messages": [
            """
            ### FINAL PRODUCT TARGET: FACTORIO ARCHITECT (OFFLINE MODE)

            **Objective:** Deliver a complete, version-controlled C# Avalonia application that models Factorio production chains via a graphical UI and an integrated MCP server.

            **1. Core Simulation & Data Logic**
            - **Data Source:** Use a local file `factorio_recipes_and_machines.json` (to be defined by the PO/Dev) as the single source of truth.
            - **Throughput Engine:** Implement the math for production logic: T = (Output/CraftingTime) * MachineSpeed.
            - **Targets:** Specifically support modeling 'Advanced Circuit' (10/min) and 'Express Splitter' (2.5/min).

            **2. Graphical Interface (Avalonia UI)**
            - **Scaffolding:** The project must be initialized using standard `Avalonia.Templates`.
            - **Visuals:** Implement a node-based canvas with emojis (🏭, ⚙️, 🟦) and throughput display.

            **3. MCP Server Integration**
            - Provide tools: `add_node`, `connect_nodes`, `get_bottlenecks()`.

            **4. Workflow & Initialization Mandates**
            - **Clean Start:** The project must begin with a `git init` in the source directory to establish a baseline.
            - **Versioning:** Use the `git_tool` for all changes with Conventional Commit messages (feat, fix, refactor).
            - **Context Awareness:** Agents MUST use the `DirectoryReadTool` at the start of every task to check existing files. Do not overwrite logic without understanding the existing codebase first.

            **5. Definition of Done (Documentation)**
            - **Root README.md:** Kept by PO.
            - **Technical READMEs:** Kept by Dev in every subsystem folder.
            """
        ],
        "next_node": "product_owner",
        "role_violation_flag": False,
        "qa_dev_iterations": 0, # Initialize the tracking variable
        "product_backlog": [],
        "sprint_backlog": [],
        "current_increment": {
            "specs": "",
            "code": ""
        },
        "qa_results": {}
    }

    print("--- MAGE-SCRUM RUNTIME STARTED (OFFLINE MODE) ---")
    for event in scrum_app.stream(initial_state):
        for node, update in event.items():
            print(f"\n[NODE]: {node}")
            if "messages" in update:
                print(f"[LOG]: {update['messages'][-1]}")

            # Print Telemetry
            print(
                f"[METRICS] Total Tokens: {metrics.total_tokens} | Time: {metrics.total_time:.2f}s | Requests: {metrics.calls}")
