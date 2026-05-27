[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/kz4Hl53G)
## Infrastructure Setup: Local LLM

### Prerequisites
- **Runner:** Ollama v0.1.x+
- **Model:** `qwen2.5-coder:latest` (modified)

### Model Config
```
FROM qwen2.5-coder:latest

PARAMETER temperature 0.2
PARAMETER top_p 0.9
PARAMETER repeat_penalty 1.1
PARAMETER num_ctx 8192
PARAMETER num_predict 2048
PARAMETER stop "</s>"
PARAMETER stop "user:"
PARAMETER stop "\n\n\n"
```

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
