# FACTORIO ARCHITECT - Technical Architecture

## 🎯 Project Overview

**Factorio Architect** is a complete, version-controlled C# Avalonia application that models Factorio production chains via a graphical UI and an integrated MCP (Model Context Protocol) server. It works offline using a local JSON recipe database.

## 🏗️ System Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                        FACTORIO ARCHITECT                           │
├────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐    │
│  │ Avalonia UI     │  │  Simulation     │  │  MCP Server     │    │
│  │   Layer         │  │   Engine        │  │   Integration    │    │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘    │
│          │                  │                  │                    │
│          ▼                  ▼                  ▼                    │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ factorio_recipes_and_machines.json                          │  │
│  │                    (Single Source of Truth)                 │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
└────────────────────────────────────────────────────────────────────┘
```

## 📁 Project Structure

```
src/
├── AvaloniaUI/                   # Graphical interface subsystem
│   ├── README.md
│   ├── Views/
│   ├── Models/
│   ├── Controls/
│   └── Resources/
├── SimulationEngine/             # Throughput calculation subsystem
│   ├── README.md
│   ├── Models/
│   ├── Services/
│   └── Interfaces/
├── MCPServer/                    # Tool server subsystem
│   ├── README.md
│   ├── Tools/
│   ├── State/
│   └── Models/
└── Resources/
    └── factorio_recipes_and_machines.json
```

## 🔄 Data Flow

```
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│   User/Tool  │───►──►│ AvaloniaUI   │───►──►│Visualization │
│   Input      │◄─────│   Layer      │◄──────│   Output    │
└──────────────┘      └──────────────┘       └──────────────┘
                                              ▲
                                              │
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│  JSON Data   │───►──►│Simulation    │───►──►│ Bottleneck  │
│   Source     │◄─────│   Engine     │◄──────│   Analysis  │
└──────────────┘       └──────────────┘       └──────────────┘
                                              ▲
                                              │
┌──────────────┐                              │
│  MCP Tools   │                              │
└──────────────┘                              │
```

## 📊 Throughput Calculation

The core formula for production rate calculation:

```
T = (Output / CraftingTime) × MachineSpeed
```

Where:
- **T** = Throughput (items per minute)
- **Output** = Items produced per crafting cycle
- **CraftingTime** = Seconds required for one crafting cycle
- **MachineSpeed** = Production speed multiplier (default 1.0)

## 🧪 Gherkin Acceptance Criteria Summary

| Feature | Stories | Key Scenarios |
|---------|---------|---------------|
| **Core Simulation & Data Logic** | 4 | Local data load, throughput calculation, batch calculation, error handling |
| **Graphical Interface (Avalonia UI)** | 3 | Node creation, emoji rendering, throughput display |
| **MCP Server Integration** | 3 | Tool implementations with full parameter validation |
| **Workflow & Initialization** | 3 | Git initialization, Conventional Commits, DirectoryReadTool protocol |
| **Definition of Done** | 2 | Documentation maintenance, version strategy |

**Total: 15+ scenarios with detailed step-by-step acceptance criteria**

## 🛠️ MCP Tools Available

### `add_node`
Creates a new production node in the simulation graph.

**Parameters:**
- `node_id` (string): Unique identifier for the node
- `emoji` (string): Emoji icon for visual representation
- `label` (string): Display label for the node  
- `recipe_id` (string): Recipe identifier from the database

### `connect_nodes`
Connects two nodes with a production line and ratio.

**Parameters:**
- `from` (string): Source node ID
- `to` (string): Target node ID
- `ratio` (number): Production ratio (amount per 1 item)

### `get_bottlenecks`
Analyzes the production chain and returns bottleneck information.

**Returns:**
- List of bottlenecks with severity ranking (Critical, Warning, Info, Optimal)
- Efficiency percentage for each machine
- Recommendations for improvement

## 🔄 Git Workflow

### Standard .gitignore
```
bin/
obj/
*.pkl
.vs/
.idea/
*.user/
```

### Pre-commit Hooks
- Linting (Roslynator)
- Testing (dotnet test)
- Formatting (Roslynator.Formatting.MSBuild)

### Conventional Commits
```
feat(ui): add throughput card component
fix(engine): fix throughput calculation overflow
docs(mcp): document new tool parameters
refactor(ui): extract node view to separate component
```

### Branch Strategy
```
main                          # Stable release branch
├── develop                    # Integration branch
│   ├── feature/ui-1           # New feature branches
│   ├─ feature/engine-1
│   └── hotfix/ui-1
└── feature/ui-next-version   # Next release prep
```

## 📚 Documentation Strategy

- **Product Owner**: Maintains Root README.md (vision & requirements)
- **Developers**: Maintain Technical READMEs per subsystem
- **Version**: 0.1.0-alpha

## 🎯 Core Targets Supported

| Target | Rate | Description |
|--------|------|-------------|
| Advanced Circuit Boards | 10/min | High-speed logic production |
| Express Splitters | 2.5/min | Fast divider production |
| Full Production Chain | Variable | Complete modeling capability |

## 🔐 Security & Compliance

- **Offline Mode**: No external data required
- **Input Validation**: All tool parameters validated
- **XSS Prevention**: ContentSecurityPolicy configured
- **Rate Limiting**: DoS protection enabled

## 📝 Version History

| Version | Date | Owner | Changes |
|---------|------|-------|---------|
| 0.1.0-alpha | 2024-01 | Developer Team | Initial Technical Architecture |

---

**Documented By**: Developer Team  
**Reviewed By**: Product Owner  
**Last Updated**: 2024-01  
