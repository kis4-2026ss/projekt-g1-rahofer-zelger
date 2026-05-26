import os
from langgraph.graph import StateGraph, END
from crewai import Agent, Task
from crewai_tools import FileReadTool, FileWriterTool, DirectoryReadTool
from langchain_openai import ChatOpenAI
from typing import Dict, List, Annotated
from operator import add
from typing_extensions import TypedDict

class ScrumState(TypedDict):
    next_node: str
    role_violation_flag: bool
    messages: Annotated[list, add]

    #artifacts
    product_backlog: List[Dict[str, str]]
    sprint_backlog: List[Dict[str, str]]
    current_increment: Dict[str, str]
    qa_results: Dict[str, any]

#GLOBALS
LLM_STR = "openai/openai/gpt-oss-120b"
URL_STR = "http://127.0.0.1:18000/v1"
API_KEY = "not-needed"

file_writer = FileWriterTool()
file_reader = FileReadTool()
dir_reader = DirectoryReadTool()

os.environ["OPENAI_API_KEY"] = API_KEY
os.environ["OPENAI_API_BASE"] = URL_STR
os.environ["OTEL_SDK_DISABLED"] = "true"

product_owner = Agent(
    role="Product Owner",
    goal="Translate raw feature concepts into clear user stories and Gherkin definitions.",
    backstory="""You are a strict Product Owner. Your authority is limited to the 'WHAT' and 'WHY'. 
    You excel at behavior-driven development (BDD). You define explicit rules for success.""",
    llm=LLM_STR,
    function_calling_llm=LLM_STR,
    base_url=URL_STR,
    tools=[file_writer, dir_reader],
    verbose=True,
    streaming=True,
    allow_delegation=False,
    system_template="""
    {system_message}

    CRITICAL PROTOCOL:
    1. You only talk about requirements, user goals, and features.
    2. You write exclusively in Gherkin syntax (Given, When, Then).

    STRICT NEGATIVE CONSTRAINTS:
    * DO NOT write Python code, pseudocode, or execution configurations.
    * DO NOT specify functions, class structures, or database schemas.
    * If asked to fix code, respond with: 'As a Product Owner, I am prohibited from writing or adjusting codebase architecture.'
    """
)

scrum_master = Agent(
    role="Scrum Master",
    goal="Ensure strict adherence to Scrum protocols and prevent role-bleeding between agents.",
    backstory="""You are a veteran Agile Coach and Scrum Master. You do not build products; 
    you build the process. You are hyper-vigilant about 'scope creep' and 'role violations'. 
    If a Developer tries to change a requirement, or a Product Owner tries to suggest code, 
    you are the one who blocks the action and enforces the rules.""",
    llm=LLM_STR,
    function_calling_llm=LLM_STR,
    base_url=URL_STR,
    tools=[file_writer, dir_reader],
    verbose=True,
    streaming=True,
    allow_delegation=False,
    system_template="""
    {system_message}

    CRITICAL PROTOCOL:
    1. Your primary output is "Audit Reports" (which you save as .txt files) regarding the team's interaction.
    2. You must flag any instance where an agent performs a task outside their restricted field.
    3. You evaluate if the Product Owner's Gherkin is clear enough for the Developer to begin.

    STRICT NEGATIVE CONSTRAINTS:
    * DO NOT write, edit, or suggest Python code or any technical implementation.
    * DO NOT create, delete, or modify Product Requirements (User Stories).
    * DO NOT perform testing or QA validation on the software increment.
    * If asked to help with coding or features, respond with: 'As Scrum Master, my focus is exclusively on process integrity and clearing blockers; I cannot participate in technical implementation or requirement definition.'
    """
)

developer = Agent(
    role="Developer",
    goal="Write complete, functional Python source code matching the Product Owner's specifications.",
    backstory="""You are a precise Software Engineer. You write clean, modular, and PEP8-compliant Python code. 
    You do not discuss product value, change acceptance criteria, or negotiate requirements. You simply implement rules.""",
    llm=LLM_STR,
    function_calling_llm=LLM_STR,
    base_url=URL_STR,
    tools=[file_writer, dir_reader],
    verbose=True,
    streaming=True,
    allow_delegation=False,
    system_template="""
    {system_message}

    CRITICAL PROTOCOL:
    1. You only write valid, syntax-correct Python applications and code structures.
    2. Your application design must follow the feature definitions provided by the Product Owner.

    STRICT NEGATIVE CONSTRAINTS:
    * DO NOT invent features, modify business scope, or drop explicit acceptance criteria.
    * DO NOT create test plans, QA strategies, or mock test files (that is the QA Tester's responsibility).
    * If you believe a requirement is missing or ambiguous, do not patch it yourself; instead output: 'BLOCKED: Ambiguous requirements.'
    """
)

qa_tester = Agent(
    role="QA Tester",
    goal="Develop structural automated testing frameworks and identify functional edge-case faults.",
    backstory="""You are an analytical, skeptical Quality Assurance Engineer. You write test scripts 
    (such as pytest suites) based on Gherkin feature definitions to catch implementation bugs in the source code.""",
    llm=LLM_STR,
    function_calling_llm=LLM_STR,
    base_url=URL_STR,
    tools=[file_writer, dir_reader],
    verbose=True,
    streaming=True,
    allow_delegation=False,
    system_template="""
    {system_message}

    CRITICAL PROTOCOL:
    1. You only write test suites (e.g., using pytest or unittest frameworks) and document code defects.
    2. Your tests must explicitly trace back to the Product Owner's Gherkin behavioral specifications.

    STRICT NEGATIVE CONSTRAINTS:
    * DO NOT fix bugs, edit application source code, or adjust implementation architectures.
    * DO NOT alter product scope or accept features that break the defined Gherkin workflows.
    * If a test fails, do not rewrite the source file yourself; output a specific bug diagnostic detailing where the code broke.
    """
)

def product_owner_node(state: ScrumState) -> Dict:
    latest_input = state["messages"][-1] if state["messages"] else ""

    task = Task(
        description=f"Analyze input: {latest_input}. Generate feature specifications.",
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
    latest_code = state["current_increment"].get("code", "")

    if latest_specs and not latest_code:
        audit_context = f"""
            Review the Product Owner's specifications:
            {latest_specs}

            Verify that it contains ONLY Gherkin requirements. If there is ANY raw Python 
            code, class structures, or database schemas, reject it.
            Otherwise, reply exactly with: 'PROCEED'
            """
        next_destination = "developer"

    else:
        audit_context = f"""
            Review the Developer's code against the PO's specs:
            SPECS: {latest_specs}
            CODE: {latest_code}

            Verify that the Developer did not change the scope or introduce features 
            not requested by the PO.
            Otherwise, reply exactly with: 'PROCEED'
            """
        next_destination = "qa_tester"

    task = Task(
        description=audit_context,
        agent=scrum_master,
        expected_output="PROCEED or rejection log."
    )

    audit_result = str(task.execute_sync())

    if "PROCEED" in audit_result.upper():
        return {
            "messages": [f"Scrum Master approved phase."],
            "next_node": next_destination,
            "role_violation_flag": False
        }
    else:
        fallback_node = "product_owner" if not latest_code else "developer"
        return {
            "messages": [f"Scrum Master Rejected Phase: {audit_result}"],
            "next_node": fallback_node,
            "role_violation_flag": True
        }


def developer_node(state: ScrumState) -> Dict:
    specs = state["current_increment"].get("specs", "")

    latest_message = state["messages"][-1] if state["messages"] else ""
    qa_report = state.get("qa_results", {}).get("report", "")

    feedback_context = ""
    if "Rejected Phase" in latest_message:
        feedback_context = f"\nYOUR PREVIOUS CODE WAS REJECTED BY THE SCRUM MASTER. Fix it based on this feedback:\n{latest_message}"
    elif "Failed" in latest_message and qa_report:
        feedback_context = f"\nYOUR PREVIOUS CODE FAILED QA TESTING. Fix the bugs detailed in this report:\n{qa_report}"

    task_prompt = f"""
        Based on the following Product Owner specifications, develop a functional Python application:
        ---
        SPECIFICATIONS:
        {specs}
        ---
        {feedback_context}

        Generate and save the clean Python codebase implementing these features.
        Ensure you adhere strictly to the given scope and write no additional extraneous features.
        """

    task = Task(
        description=task_prompt,
        agent=developer,
        expected_output="Functional Python application files implementing the requested behavior."
    )

    code_output = str(task.execute_sync())

    return {
        "messages": ["Developer completed the application codebase increment."],
        "current_increment": {"specs": specs, "code": code_output},
        "next_node": "scrum_master"
    }


def qa_tester_node(state: ScrumState) -> Dict:
    specs = state["current_increment"].get("specs", "")
    code = state["current_increment"].get("code", "")

    task_prompt = f"""
    Review the application source code against the original Gherkin behavioral specs:
    ---
    SPECIFICATIONS:
    {specs}
    ---
    DEVELOPER CODEBASE:
    {code}
    ---

    Construct a python test suite to verify this code. 
    Verify that all constraints are met. Detail any uncovered errors or edge cases.
    If everything passes successfully without defects, include 'QA_PASSED' in your final response.
    """

    task = Task(
        description=task_prompt,
        agent=qa_tester,
        expected_output="An analytical test suite summary/execution script or a detailed bug defect report."
    )

    qa_output = str(task.execute_sync())

    qa_results = {
        "passed": "QA_PASSED" in qa_output.upper(),
        "report": qa_output
    }

    next_step = "end" if qa_results["passed"] else "developer"

    return {
        "messages": [f"QA Execution Result: {'Passed' if qa_results['passed'] else 'Failed - Re-routing back.'}"],
        "qa_results": qa_results,
        "next_node": next_step
    }

builder = StateGraph(ScrumState)

builder.add_node("product_owner", product_owner_node)
builder.add_node("scrum_master", scrum_master_node)
builder.add_node("developer", developer_node)
builder.add_node("qa_tester", qa_tester_node)

builder.set_entry_point("product_owner")

def routing_router(state: ScrumState) -> str:
    return state.get("next_node", "end")

builder.add_conditional_edges(
    "product_owner",
    routing_router,
    {"scrum_master": "scrum_master", "end": END}
)

builder.add_conditional_edges(
    "scrum_master",
    routing_router,
    {
        "developer": "developer",
        "qa_tester": "qa_tester",
        "product_owner": "product_owner",
        "end": END
    }
)

builder.add_conditional_edges(
    "developer",
    routing_router,
    {"scrum_master": "scrum_master", "end": END}
)

builder.add_conditional_edges(
    "qa_tester",
    routing_router,
    {
        "scrum_master": "scrum_master",
        "developer": "developer",
        "end": END
    }
)

scrum_app = builder.compile()
