import crew_assembly

if __name__ == "__main__":
    initial_sprint_state = {
        "messages": ["Create A simple python program containing a ui button which prints hello world. Save this program locally in a folder called 'Result'."],
        "product_backlog": [],
        "sprint_backlog": [],
        "current_increment": {"specs": "", "code": ""},
        "next_node": "product_owner"
    }

    print("==============================================")
    print("STARTING MULTI-AGENT SCRUM RUNTIME ENVIRONMENT")
    print("==============================================\n")

    for event in crew_assembly.scrum_app.stream(initial_sprint_state):
        for node_name, state_update in event.items():
            print(f"\n[Finished Execution Node]: {node_name}")
            if "current_increment" in state_update:
                specs_preview = state_update["current_increment"].get("specs", "")[:200]
                print(f"[State Update Payload Preview]:\n{specs_preview}...")