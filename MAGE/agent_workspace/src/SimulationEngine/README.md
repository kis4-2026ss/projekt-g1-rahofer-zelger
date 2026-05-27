# Simulation Engine Subsystem

## 📋 Overview

The **Simulation Engine** subsystem implements the core throughput calculation logic for the Factorio Architect application. It models production chains using the offline recipe database and performs real-time bottleneck analysis.

## 🎯 Core Responsibilities

- Load and validate recipe data from JSON
- Calculate throughput using formula: `T = (Output/CraftingTime) × MachineSpeed`
- Model production chain dependencies
- Perform simulation runs (batch or real-time)
- Identify and rank bottlenecks
- Calculate required input rates for targets

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Simulation Engine                       │
├─────────────────────────────────────────────────────────┤
│  ┌───────────────────┐   ┌───────────────────┐            │
│  │  Recipe Database  │   │  Throughput Model │            │
│  │  (JSON Parser)    │   │  (Calculator)     │            │
│  └───────────────────┘   └───────────────────┘            │
│           │                      │                         │
│  ┌───────────────────┐   ┌───────────────────┐            │
│  │  Dependency Graph │   │  Bottleneck       │            │
│  │  (GraphBuilder)   │   │  Analyzer         │            │
│  └───────────────────┘   └───────────────────┘            │
│           │                      │                         │
│  └────────────────────────────────────────────────┘        │
│                        Simulation Results                   │
└────────────────────────────────────────────────────────────┘
```

## 📁 File Structure

```
SimulationEngine/
├── README.md                              # This file
├── Config/
│   └── recipe_database.json               # factorio_recipes_and_machines.json
├── Models/
│   ├── Recipe.cs                          # Recipe data model
│   ├── Machine.cs                         # Machine production model
│   ├── ProductionChain.cs                 # Chain dependency graph
│   └── SimulationState.cs                  # State management
├── Services/
│   ├── RecipeLoader.cs                    # JSON file loading
│   ├── ThroughputCalculator.cs            # T = Output/(Time * Speed)
│   ├── DependencyGraphBuilder.cs          # Graph construction
│   └── BottleneckAnalyzer.cs              # Severity ranking
├── Interfaces/
│   ├── IRecipeLoader.cs
│   ├── IThroughputCalculator.cs
│   └── IBottleneckAnalyzer.cs
├── Tests/
│   └── SimulationEngine.Tests/
└── Calculators/
    └── CircuitBoardCalculator.cs          # Specialized targets
```

## 🧪 Gherkin Acceptance Criteria

### Feature: Local Data Load

```gherkin
Feature: Local Data Source Loading
  Background:
    Given the application is starting up
    And a file "factorio_recipes_and_machines.json" exists

  Scenario: Load full recipe database
    When the application starts
    Then it reads the JSON file
    And populates the internal recipe database
    And validates the JSON schema
    And logs any parsing errors

  Scenario: Handle missing file
    When the required JSON file is not found
    Then the application logs a warning
    And starts with empty recipe database
    And shows a "Install Recipes" banner

  Scenario: Validate recipe data structure
    Given the JSON file contains invalid recipe data
    When the application attempts to load it
    Then it skips invalid recipes
    And logs which recipes were skipped
    And provides a detailed error report
```

### Feature: Throughput Calculation

```gherkin
Feature: Throughput Calculation
  Background:
    Given a machine with defined recipe and speed

  Scenario: Calculate basic throughput
    When a machine processes 100 items in 10 minutes
    And machine speed is 1.0
    Then throughput = (100/10) × 1.0 = 10 items/min

  Scenario: Handle crafting time
    When recipe crafting time = 5 seconds
    And yield = 100 items
    And machine speed = 1.0
    Then throughput = (100/items) × (3600/s)
    And result is displayed as items/hour

  Scenario: Batch calculation
    When simulating a production chain
    Then it calculates throughput for each machine
    And identifies the bottleneck
    And calculates required input rates
```

### Feature: Bottleneck Analysis

```gherkin
Feature: Bottleneck Analysis
  Background:
    Given a production chain is simulated

  Scenario: Identify bottlenecks
    When calculating the entire chain
    Then it finds the machine with lowest throughput
    And ranks bottlenecks by severity
    And suggests solutions

  Scenario: Severity ranking
    Given multiple production machines
    When calculating chain efficiency
    Then it assigns severity:
    - Critical: <50% efficiency
    - Warning: 50-70% efficiency
    - Info: >70% efficiency
```

## 🔄 Git Workflow

### Conventional Commits for Engine Changes

```
feat(engine): add circuit board calculator
fix(engine): fix throughput calculation overflow
docs(engine): document dependency graph algorithms
refactor(engine): extract bottleneck analyzer to separate class
```

### Branch Strategy

```
main                          # Stable release branch
├── develop                    # Integration branch
│   ├── feature/engine-1       # Engine feature branches
│   └── hotfix/engine-1
└── feature/engine-next        # Next release prep
```

## 📐 Technical Specifications

### Throughput Formula

```csharp
public double CalculateThroughput(Recipe recipe, double machineSpeed)
{
    double craftingTimeSeconds = recipe.RecipeTime / 100.0; // JSON stores as ms
    double speedMultiplier = machineSpeed;
    
    // Items per crafting cycle
    double itemsPerCycle = recipe.Yield;
    
    // Cycle time in minutes
    double cycleTimeMinutes = craftingTimeSeconds / 60.0;
    
    // Throughput = (items per cycle) / (cycle time in minutes) * speed
    double throughput = (itemsPerCycle / cycleTimeMinutes) * speedMultiplier;
    
    return throughput;
}
```

### Dependency Graph

```csharp
public class ProductionGraph
{
    public Dictionary<string, List<string>> Dependencies { get; set; }
    public List<Recipe> AllRecipes { get; set; }
    
    // Build from recipe database
    public ProductionGraph(List<Recipe> recipes)
    {
        AllRecipes = recipes;
        BuildDependencyGraph(recipes);
    }
    
    protected void BuildDependencyGraph(List<Recipe> recipes)
    {
        foreach (var recipe in recipes)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (Dependencies.ContainsKey(recipe.Id))
                    Dependencies[recipe.Id].Add(ingredient.Id);
                else
                    Dependencies[recipe.Id] = new List<string> { ingredient.Id };
            }
        }
    }
}
```

### Bottleneck Severity Calculation

```csharp
public enum BottleneckSeverity
{
    Critical,   // <50% efficiency
    Warning,    // 50-70% efficiency  
    Info,       // >70% efficiency
    Optimal     // 100% (no bottleneck)
}

public (string Machine, double Rate, BottleneckSeverity Severity)[] GetBottlenecks()
{
    // Implementation details...
}
```

## 🔧 Configuration

### appsettings.json

```json
{
  "SimulationEngine": {
    "BottleneckThreshold": 0.7,
    "CalculationPrecision": 4,
    "DefaultMachineSpeed": 1.0,
    "MaxChainDepth": 10,
    "BatchSize": 100,
    "EnableRealTimeMode": false
  }
}
```

## 🧰 Dependencies

- System.Data (for graph operations)
- MathNet.Numerics (for optimization)
- (None - pure calculation, no external deps)

## 🔐 Security

- No external data (offline mode)
- JSON Schema validation prevents injection
- Input sanitization on recipe IDs

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.1.0-alpha | 2024-01 | Initial throughput calculation |

---

**Owner**: Developer Team  
**Review By**: Product Owner  
**Last Updated**: 2024-01  
