import crew_assembly
import sys

if __name__ == "__main__":
    print("==================================================================")
    print("         MAGE-Scrum: Phase 1 & 2 Execution Environment            ")
    print("==================================================================")

    # 1. Capture user requirements
    customer_request = input("\nCustomer Prompt (e.g., 'Model an express splitter'): ").strip()
    if not customer_request:
        print("No input provided. Exiting.")
        sys.exit(0)

    initial_sprint_state = {
        "messages": [customer_request],
        "current_increment": {"specs": "", "code": ""},
        "next_node": "product_owner"
    }

    print("\n[MAGE-Scrum] Starting Phase 1: Planning Loop...\n")

    # 2. Execute Phase 1 Graph
    for event in crew_assembly.scrum_app.stream(initial_sprint_state):
        for node_name, state_update in event.items():
            print(f"[{node_name.upper()} COMPLETED TASK]")

    # 3. Phase 2: Human-in-the-Loop Interruption
    print("\n" + "=" * 66)
    print(" PHASE 2: SPRINT PLANNING COMPLETE & HALTED ")
    print("=" * 66)
    print("The team has negotiated and generated your user stories.")
    print("-> Check your local directory: shared_workspace/issues/")
    print("=" * 66)

    feedback = input("\nDo you approve this backlog to proceed to Phase 3 Implementation? (yes/no): ").strip().lower()

    if feedback == 'yes':
        print("\n[MAGE-Scrum] Backlog approved. (Phase 3 Dev/QA integration pending...)")
        sys.exit(0)
    else:
        print("\n[MAGE-Scrum] Backlog rejected. Please adjust requirements and run again.")
        sys.exit(0)