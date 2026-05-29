# FACTORIO ARCHITECT - Product Owner Specification

## 📋 System Vision

**Factorio Architect** is an offline C# Avalonia application designed to model and analyze Factorio production chains through an interactive graphical interface and intelligent bottleneck detection. The system provides a comprehensive tool for players and developers to simulate, optimize, and validate production networks without requiring internet connectivity.

### Core Value Proposition
- **Visualization:** Node-based canvas with real-time throughput metrics
- **Math-Driven:** Automated production calculations using verified throughput formulas
- **MCP Integration:** Allows external AI assistants to interact with the simulation
- **Version-Controlled:** Git-managed development workflow for maintainability

---

## 🎯 Gherkin Test Specifications

### Target: Advanced Circuit Production (10/min)
```gherkin
Feature: Advanced Circuit Production Chain

  Scenario Outline: Simulate Advanced Circuit Assembly
    Given I have a production chain with:
      | Node | Machine Type      | Recipe          | Speed Multiplier |
      | A    | Belt              | Stone           | 5.0              |
      | B    | Stone Mine        | Stone           | 1.0              |
      | C    | Belt              | Steel Plate     | 5.0              |
      | D    | Belt              | Iron Plate      | 5.0              |
      | E    | Iron Mine         | Iron Plate      | 4.0              |
      | F    | Circuit Assembly  | Advanced Circuit| 10.0             |
    When I calculate throughput for node "F"
    Then the output throughput should be approximately 10/min
    And the system should visualize node "F" as a bottleneck if input < 10/min

  Examples:
    | Machine Type      | Recipe          | Speed Multiplier |
    | Belt              | Stone           | 5.0              |
    | Stone Mine        | Stone           | 1.0              |
    | Belt              | Steel Plate     | 5.0              |
    | Belt              | Iron Plate      | 5.0              |
    | Iron Mine         | Iron Plate      | 4.0              |
    | Circuit Assembly  | Advanced Circuit| 10.0             |
```

### Target: Express Splitter Production (2.5/min)
```gherkin
Feature: Express Splitter Production Chain

  Scenario Outline: Simulate Express Splitter Assembly
    Given I have a production chain with:
      | Node | Machine Type    | Recipe       | Speed Multiplier |
      | A    | Belt            | Concrete     | 5.0              |
      | B    | Belt            | Concrete     | 5.0              |
      | C    | Belt            | Concrete     | 5.0              |
      | D    | Concrete Bunker | Concrete     | 2.5              |
      | E    | Splitter        | Splitter     | 2.5              |
    When I calculate throughput for node "E"
    Then the output throughput should be 2.5/min
    And the system should highlight the splitter node as the bottleneck

  Examples:
    | Machine Type    | Recipe       | Speed Multiplier |
    | Belt            | Concrete     | 5.0              |
    | Belt            | Concrete     | 5.0              |
    | Belt            | Concrete     | 5.0              |
    | Concrete Bunker | Concrete     | 2.5              |
    | Splitter        | Splitter     | 2.5              |
```

### Feature: Throughput Calculation Engine
```gherkin
Feature: Production Throughput Calculation

  Background:
    Given the throughput formula: Throughput = (Output / CraftingTime) × MachineSpeed

  Scenario: Basic machine throughput calculation
    Given a Stone Mine node with:
      | Crafting Time | Output | Machine Speed |
      | 55            | 1      | 1.0           |
    When I query the throughput for this node
    Then the throughput should equal 0.01818/min (1/55 × 1.0 × speed_multiplier)

  Scenario: Complex multi-stage assembly line
    Given a production line with machines A, B, C where:
      | Node | Crafting Time | Output | Speed |
      | A    | 10            | 5      | 5.0   |
      | B    | 20            | 3      | 4.0   |
      | C    | 15            | 2      | 6.0   |
    When I calculate the bottleneck throughput
    Then the system should identify node B as the bottleneck
    And the bottleneck throughput should be (3/20) × 4.0 = 0.6/min
```

### Feature: Bottleneck Detection
```gherkin
Feature: Bottleneck Identification via MCP

  Scenario: Starved node detection
    Given a production chain where node A produces 10/min
    And node B (downstream) has capacity of 5/min
    When I call get_bottlenecks()
    Then the system should return node B as a bottleneck with status "over-producing upstream"
    And node B should be flagged as "starved" by node A

  Scenario: Over-producing node detection  
    Given a production chain where node A produces 3/min
    And node B (downstream) has capacity of 10/min
    When I call get_bottlenecks()
    Then the system should return node A as a bottleneck with status "under-producing"
    And node A should be flagged as "limiting downstream capacity"
```

### Feature: Node Operations via MCP
```gherkin
Feature: MCP Tool Operations

  Scenario: Add node to canvas
    Given the canvas is initialized
    When I call add_node("Circuit Assembly", "Advanced Circuit")
    Then a node with ID auto-generated
    Should be created with emoji "⚙️"
    And should display throughput on input ports

  Scenario: Connect nodes together
    Given nodes A and B exist on canvas
    When I call connect_nodes("A", "B")
    Then an arrow should be drawn from A's output to B's input
    And data flow visualization should update

  Scenario: Get all bottlenecks
    Given a populated canvas with production chain
    When I call get_bottlenecks()
    Then the response should include:
      - List of bottleneck node IDs
      - Type of imbalance (starvation/overproduction)
      - Expected vs actual throughput values
```

---

## 📐 Throughput Math Specification

### Formula Components
```
Throughput = (Output_Per_Cycle / Crafting_Time) × Machine_Speed_Multiplier
```

### Variable Definitions
- **Output_Per_Cycle:** Number of items produced per crafting cycle (from recipe definition)
- **Crafting_Time:** Time in ticks to craft one unit (from recipe definition)
- **Machine_Speed_Multiplier:** Speed modifier (e.g., 5.0 for belt = 5 items/min at 1.0 base speed)

### Calculation Example: Advanced Circuit
```
Advanced Circuit Recipe:
- Crafting Time: 300 ticks (75 seconds) at full speed
- Output Per Cycle: 1 circuit
- Base Speed: 1.0 min⁻¹

Calculation:
  Throughput = (1 / 300) × 10.0 (belt multiplier)
  Throughput = 0.00333 × 10.0
  Throughput = 0.0333/min → Scale to 10/min with assembly belt system

With Assembly Belt System (optimized chain):
  Assembly Belt Speed = 5.0 (standard belt)
  Throughput = (1/300) × 5.0 = 0.0167/min per belt
  With parallel belts: 6 belts × 0.0167 = 0.1/min
  Optimized with KAS 5.0: 5.0 min⁻¹ throughput
```

---

## 🏗️ System Architecture

### Technical Stack
- **Frontend:** Avalonia UI with node-based canvas
- **Backend:** C#/.NET with recipe calculation engine
- **Data Layer:** JSON-based recipe repository (factorio_recipes_and_machines.json)
- **Integration:** MCP server for LLM interaction

### Directory Structure
```
agent_workspace/
├── src/
│   ├── FactorioArchitect.Core/          # Core simulation logic
│   ├── FactorioArchitect.UI/            # Avalonia UI layer
│   ├── FactorioArchitect.MCP/           # MCP server implementation
│   └── FactorioArchitect.Data/          # Recipe data access
├── docs/
│   ├── README.md                        # Technical documentation
│   └── API.md
├── tests/
│   ├── UnitTests/
│   └── IntegrationTests/
├── factorio_recipes_and_machines.json   # Single source of truth
└── README.md                            # This file
```

---

## 📁 Data Structure: factorio_recipes_and_machines.json

### Schema Specification
```json
{
  "recipes": [
    {
      "id": "advanced_circuit",
      "name": "Advanced Circuit",
      "output": 1,
      "craftingTimeTicks": 300,
      "requiredItems": [
        {
          "item": "Circuit Card Mk.I",
          "quantity": 1
        }
      ],
      "machineType": "Circuit Assembly Unit",
      "machineEmoji": "⚙️"
    }
  ],
  "machines": [
    {
      "id": "assembly_belt",
      "name": "Assembly Belt",
      "speedMultiplier": 5.0,
      "emoji": "🟦",
      "inputCapacity": 1,
      "outputCapacity": 1
    }
  ]
}
```

---

## ✅ Definition of Done

### MVP Requirements
- [ ] Git repository initialized with baseline commit
- [ ] `factorio_recipes_and_machines.json` defined with target recipes
- [ ] Avalonia UI canvas scaffold with node rendering
- [ ] Throughput calculation engine implemented and tested
- [ ] MCP server tools operational (add_node, connect_nodes, get_bottlenecks)
- [ ] Advanced Circuit (10/min) and Express Splitter (2.5/min) test cases pass
- [ ] Real-time throughput visualization on node ports
- [ ] Bottleneck detection algorithm verified
- [ ] Documentation complete (README, Technical docs, Gherkin specs)

### Quality Gates
- **QA_PASSED Criteria:**
  - All Gherkin scenarios pass
  - Simulation math verified against known Factorio mechanics
  - Version-controlled changes tracked via Conventional Commits
  - MCP tools functional and tested
  - UI responsive and bug-free

---

## 📝 Development Workflow

### Version Control Protocol
1. Initialize: `git init` in root directory
2. All changes: `git_tool` with Conventional Commits
3. Commit message format: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`

### Agent Responsibilities
- **Product Owner:** Maintain Root README.md, define requirements, accept Gherkin specs
- **Developer:** Implement technical details, create subsystem README.md files
- **QA:** Verify simulation accuracy and complete documentation

### Mandatory Practices
- Use `DirectoryReadTool` at task start to review existing files
- Do not overwrite code without understanding the context
- Maintain clean, version-controlled development process

---

## 🚀 Getting Started

### Initialization
```bash
cd /fml/Projects/4Semester/projekt-g1-rahofer-zelger/MAGE/agent_workspace/issues
git init
git config user.email "your@email.com"
git config user.name "Your Name"
```

### First Steps
1. Review existing files in the issues directory
2. Define `factorio_recipes_and_machines.json` structure
3. Scaffold Avalonia project with templates
4. Implement core throughput calculation engine
5. Build MCP server interfaces
6. Write and validate Gherkin test scenarios

---

## 📚 Related Documentation

- Technical Implementation: Located in `docs/README.md` per subsystem
- MCP Protocol: Standard MCP tool interfaces (add_node, connect_nodes, get_bottlenecks)
- UI Guidelines: Avalonia.Templates standard scaffolding with emoji-based node visuals

---

**Version:** 1.0  
**Last Updated:** Current development cycle  
**Status:** Active Development  
