import os
from pathlib import Path

from dotenv import load_dotenv
from langgraph.graph import StateGraph, END
from crewai import Agent, Task, LLM
from crewai_tools import FileReadTool, FileWriterTool, DirectoryReadTool
from typing import Dict, Annotated
from operator import add
from typing_extensions import TypedDict

PROJECT_ROOT = Path(__file__).resolve().parent.parent

load_dotenv(PROJECT_ROOT / ".env")

raw_issues_path = os.getenv("AGENT_ISSUES_PATH", "./agent_workspace/issues")
ISSUES_PATH = str((PROJECT_ROOT / raw_issues_path).resolve())

raw_src_path = os.getenv("AGENT_SRC_PATH", "./agent_workspace/src")
SRC_PATH = str((PROJECT_ROOT / raw_src_path).resolve())

os.environ["CREWAI_TOOLS_ALLOW_UNSAFE_PATHS"] = "true"

# Ensure directories exist
Path(ISSUES_PATH).mkdir(parents=True, exist_ok=True)
Path(SRC_PATH).mkdir(parents=True, exist_ok=True)

class ScrumState(TypedDict):
    next_node: str
    messages: Annotated[list, add]
    current_increment: Dict[str, str]

file_writer = FileWriterTool()
dir_reader = DirectoryReadTool()

# Routing to local Ollama instance via Docker host gateway
llm_config = LLM(
    model="ollama/qwen3.5-opencode:latest",
    base_url="http://localhost:11434",
    api_key="NA"
)

# --- Agent Definitions ---

product_owner = Agent(
    role="Product Owner",
    goal="Translate raw feature concepts into clear user stories and Gherkin definitions.",
    backstory=f"You are a strict Product Owner. You define explicit rules for success and write Gherkin feature files to {ISSUES_PATH}. You NEVER write code.",
    llm=llm_config,
    tools=[file_writer, dir_reader],
    verbose=True
)

scrum_master = Agent(
    role="Scrum Master",
    goal="Ensure strict adherence to Scrum protocols and prevent role-bleeding.",
    backstory=f"You are a veteran Agile Coach. You evaluate if the Product Owner's Gherkin files in {ISSUES_PATH} are clear enough. You output 'PROCEED' if the specs are pure and ready.",
    llm=llm_config,
    tools=[file_writer, dir_reader],
    verbose=True
)

developer = Agent(
    role="Developer",
    goal="Write complete, functional Python/C# source code matching the Product Owner's specifications.",
    backstory=f"""Precise Engineer. 
    1. You MUST use a static mapper function for data transformations.
    2. The 'id' in customer data is double data but is REQUIRED if no address is retrieved.
    You write source code exclusively to {SRC_PATH}.""",
    llm=llm_config,
    tools=[file_writer, dir_reader],
    verbose=True
)

qa_tester = Agent(
    role="QA Tester",
    goal="Develop structural automated testing frameworks.",
    backstory=f"Analytical skeptic. You read specs from {ISSUES_PATH} and code from {SRC_PATH}. You use tools to verify code.",
    llm=llm_config,
    tools=[dir_reader],
    verbose=True
)

# --- Node Execution Functions ---

def product_owner_node(state: ScrumState) -> Dict:
    latest_input = state["messages"][-1] if state["messages"] else ""

    # Use the variable ISSUES_PATH here instead of /workspace/issues/
    task = Task(
        description=(
            f"Analyze input: {latest_input}. Generate explicit Gherkin feature "
            f"specifications and save them to {ISSUES_PATH}/."
        ),
        agent=product_owner,
        expected_output="Gherkin feature files"
    )

    output = str(task.execute_sync())
    return {
        "messages": [f"POA Output: {output}"],
        "current_increment": {"specs": output},
        "next_node": "scrum_master"
    }


def scrum_master_node(state: ScrumState) -> Dict:
    latest_specs = state["current_increment"].get("specs", "")

    # It helps to tell the Scrum Master where to look
    audit_context = f"""
        Review the Product Owner's specifications found in {ISSUES_PATH}:
        {latest_specs}

        Verify that it contains ONLY Gherkin requirements. If there is ANY raw Python 
        code, class structures, or database schemas, reject it.
        Otherwise, reply exactly with: 'PROCEED'
        """
    task = Task(
        description=audit_context,
        agent=scrum_master,
        expected_output="PROCEED or rejection log."
    )
    audit_result = str(task.execute_sync())

    if "PROCEED" in audit_result.upper():
        return {"messages": ["Scrum Master approved phase."], "next_node": "end"}
    else:
        return {"messages": [f"Scrum Master Rejected Phase: {audit_result}"], "next_node": "product_owner"}

# --- Graph Assembly (Scoped to Phase 1) ---
builder = StateGraph(ScrumState)
builder.add_node("product_owner", product_owner_node)
builder.add_node("scrum_master", scrum_master_node)
builder.set_entry_point("product_owner")

def routing_router(state: ScrumState) -> str:
    return state.get("next_node", "end")

builder.add_conditional_edges("product_owner", routing_router, {"scrum_master": "scrum_master", "end": END})
builder.add_conditional_edges("scrum_master", routing_router, {"product_owner": "product_owner", "end": END})

scrum_app = builder.compile()