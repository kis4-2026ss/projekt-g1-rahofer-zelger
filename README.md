[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/kz4Hl53G)
## Infrastructure Setup: Local LLM

### Prerequisites
- **Runner:** Ollama v0.1.x+
- **Model:** `carstenuhlig/omnicoder-2-9b:latest` (fine tuned qwen3.5-9b)

### Model Config
```
FROM carstenuhlig/omnicoder-2-9b:latest

# Core Parameters
PARAMETER temperature 0.2
PARAMETER top_p 0.9
PARAMETER repeat_penalty 1.1
PARAMETER num_ctx 16384
PARAMETER num_predict 4096

# Crucial Stop Tokens
PARAMETER stop "<|im_start|>"
PARAMETER stop "<|im_end|>"
PARAMETER stop "Observation:"

SYSTEM """
You are a specialized agent in a Multi-Agent Scrum Simulation.
You MUST use the following ReAct format to use tools:

Thought: [Your reasoning about what to do]
Action: [The exact tool name]
Action Input: {"arg_name": "value"}

Once you have the answer, use:
Thought: I have the final answer.
Final Answer: [Your full response]

IMPORTANT: Never output a tool call inside a markdown code block. Never explain your tool choice before the 'Thought:' header.
"""
```

`ollama create omnicoder-crew -f ./Modelfile`

### Network Configuration
To allow the agents to communicate with the model provider, the following environment variables are set:
- `OLLAMA_HOST=0.0.0.0`: Binds the server to all network interfaces.
- `OLLAMA_ORIGINS=*`: Allows Cross-Origin Resource Sharing (CORS) for agent tools.

### Firewall Rules
| Protocol | Port  | Description          | Scope         |
|----------|-------|----------------------|---------------|
| TCP      | 11434 | Ollama API Endpoint | Local Network |

### Connection String
The orchestrator connects via: `http://localhost:11434`

### Starting the Application

Start the full environment:

`docker compose up --build -d`

Restart the environment cleanly:

`docker compose down --remove-orphans`

`docker compose up --build -d`

Attach to the orchestrator container logs/output:

`docker attach mage_scrum_orchestrator`
