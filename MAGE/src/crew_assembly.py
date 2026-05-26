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

# --- 1. Path & Environment Setup ---
PROJECT_ROOT = Path(__file__).resolve().parent.parent
load_dotenv(PROJECT_ROOT / ".env")

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
        # Extract usage if provided by the LLM response
        usage = getattr(result_obj, 'token_usage', None)
        if usage:
            self.total_tokens += usage.total_tokens


metrics = MetricsTracker()

base_shell = ShellTool()


class GitTool(BaseTool):
    name: str = "git_tool"
    description: str = "Manage repository history. Usage: git_tool._run('commit -m \"feat: msg\"')"

    def _run(self, command: str) -> str:
        if any(forbidden in command for forbidden in ["push", "pull", "remote"]):
            return "Error: Remote operations are disabled for security."
        return base_shell.run(f"git -C {SRC_PATH} {command}")


class DotNetTool(BaseTool):
    name: str = "dotnet_tool"
    description: str = "C# lifecycle management (build, test, run). Usage: dotnet_tool._run('build')"

    def _run(self, command: str) -> str:
        full_cmd = f"dotnet {command}" if not command.startswith("dotnet") else command
        return base_shell.run(f"cd {SRC_PATH} && {full_cmd}")


git = GitTool()
dotnet = DotNetTool()
file_tools = [FileReadTool(), FileWriterTool(), DirectoryReadTool()]


# --- 4. State Definition ---

class ScrumState(TypedDict):
    next_node: str
    messages: Annotated[list, add]
    current_increment: Dict[str, str]


# --- 5. Agent Definitions (Strict Project Focus) ---

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
    4. Implement math: $T = \frac{\text{Recipe Output}}{\text{Crafting Time}} \times \text{Machine Speed}$."""
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


# --- 6. Node Logic ---

def execute_with_telemetry(agent, task_desc):
    start = time.time()
    task = Task(description=task_desc, agent=agent, expected_output="Defined artifact.")
    res_obj = task.execute_sync()
    duration = time.time() - start
    metrics.update(res_obj, duration)
    return str(res_obj)


def product_owner_node(state: ScrumState) -> Dict:
    prompt = f"Analyze requirements. Update Gherkin specs and Root README in {ISSUES_PATH} for: {state['messages'][-1]}"
    output = execute_with_telemetry(product_owner, prompt)
    return {"messages": [f"POA: {output}"], "current_increment": {"specs": output}, "next_node": "scrum_master"}


def scrum_master_node(state: ScrumState) -> Dict:
    specs = state["current_increment"].get("specs", "")
    code = state["current_increment"].get("code", "")
    prompt = f"Audit: Specs={specs}, Code={code}. Verify READMEs and check for role violations."
    res = execute_with_telemetry(scrum_master, prompt)

    if "PROCEED" in res.upper():
        return {"messages": ["SMA: Phase Approved."], "next_node": "developer" if not code else "qa_tester"}
    return {"messages": [f"SMA: Rejected - {res}"], "next_node": "product_owner" if not code else "developer"}


def developer_node(state: ScrumState) -> Dict:
    prompt = f"Review {SRC_PATH}. Implement: {state['current_increment']['specs']}. Update Subsystem READMEs and Commit via Git."
    output = execute_with_telemetry(developer, prompt)
    return {"messages": ["DA: Work complete."], "current_increment": {**state["current_increment"], "code": output},
            "next_node": "scrum_master"}


def qa_tester_node(state: ScrumState) -> Dict:
    prompt = f"Test the implementation in {SRC_PATH} against specs: {state['current_increment']['specs']}."
    output = execute_with_telemetry(qa_tester, prompt)
    passed = "QA_PASSED" in output.upper()
    return {"messages": [f"QA Result: {output}"], "next_node": "end" if passed else "developer"}


# --- 7. Graph Assembly ---

builder = StateGraph(ScrumState)
builder.add_node("product_owner", product_owner_node)
builder.add_node("scrum_master", scrum_master_node)
builder.add_node("developer", developer_node)
builder.add_node("qa_tester", qa_tester_node)

builder.set_entry_point("product_owner")


def router(state: ScrumState): return state.get("next_node", "end")


builder.add_conditional_edges("product_owner", router, {"scrum_master": "scrum_master", "end": END})
builder.add_conditional_edges("scrum_master", router,
                              {"developer": "developer", "qa_tester": "qa_tester", "product_owner": "product_owner",
                               "end": END})
builder.add_conditional_edges("developer", router, {"scrum_master": "scrum_master", "end": END})
builder.add_conditional_edges("qa_tester", router, {"developer": "developer", "end": END})

scrum_app = builder.compile()

# --- 8. Main Loop ---

if __name__ == "__main__":
    initial_state = {
        "messages": [
            """
            ### FINAL PRODUCT TARGET: FACTORIO ARCHITECT (OFFLINE MODE)

            **Objective:** Deliver a complete, version-controlled C# Avalonia application that models Factorio production chains via a graphical UI and an integrated MCP server.

            **1. Core Simulation & Data Logic**
            - **Data Source:** Use a local file `factorio_recipes_and_machines.json` (to be defined by the PO/Dev) as the single source of truth for recipes and machine tiers.
            - **Throughput Engine:** Implement the math for production logic using the formula:
              $$Throughput = \\frac{Output}{CraftingTime} \\times MachineSpeed$$
            - **Targets:** Specifically support modeling 'Advanced Circuit' (10/min) and 'Express Splitter' (2.5/min) production chains as the first test cases.

            **2. Graphical Interface (Avalonia UI)**
            - **Scaffolding:** The project must be initialized using standard `Avalonia.Templates`.
            - **Visuals:** Implement a node-based canvas. Nodes must use standard emojis (🏭, ⚙️, 🟦) and show real-time throughput on Input/Output ports.
            - **Interactivity:** Support connectivity between nodes representing product flow.

            **3. MCP Server Integration**
            - Provide an MCP interface allowing external LLMs to interact with the model via tools:
              - `add_node(string machineType, string recipe)`
              - `connect_nodes(string sourceId, string targetId)`
              - `get_bottlenecks()`: Identifies starved or over-producing nodes.

            **4. Workflow & Initialization Mandates**
            - **Clean Start:** The project must begin with a `git init` in the source directory to establish a baseline.
            - **Versioning:** Use the `git_tool` for all changes with Conventional Commit messages (feat, fix, refactor).
            - **Context Awareness:** Agents MUST use the `DirectoryReadTool` at the start of every task to check existing files. Do not overwrite logic without understanding the existing codebase first.

            **5. Definition of Done (Documentation)**
            - **Root README.md:** The Product Owner must maintain this in the issues folder, describing the system vision and throughput math.
            - **Technical READMEs:** The Developer must provide a README.md in every C# project/subsystem folder explaining the implementation and class structure.
            - **QA:** 'QA_PASSED' is only achieved if the simulation math is verified and the documentation suite is complete.

            **Constraint:** Do not wait for user instructions. Use your tools and the Scrum framework to self-initialize, build, test, and document the entire system.
            """
        ],
        "current_increment": {
            "specs": "",
            "code": ""
        },
        "metrics_summary": {
            "total_tokens": 0,
            "total_time": 0.0,
            "calls": 0
        },
        "next_node": "product_owner"
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