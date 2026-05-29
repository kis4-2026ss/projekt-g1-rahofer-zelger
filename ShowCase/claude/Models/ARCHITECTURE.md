# Factorio Modeler.Engine - Technical Architecture

## System Overview

The **Factorio Modeler.Engine** is the deterministic computational backend of the Factorio Modeler application. It provides recipe deserialization, throughput calculations using Factorio's tick-based simulation mechanics, machine speed tier application, bottleneck detection, and production planning algorithms.

**Key Capabilities:**
- JSON-to-object deserialization for recipes/machines
- Throughput calculation with speed tier scaling
- Bottleneck detection using critical path analysis
- Production chain planning and optimization
- Machine category management

---

## Throughput Formula

Factorio uses a tick-based time system. The default tick duration is **15.0 seconds**.

**Standard Formula:**
```
T = (OutputQty / CraftingTime) × BaseSpeed × 60
```

**Where:**
| Variable | Type | Description |
|----------|------|-------------|
| T | units/min | Throughput per minute |
| OutputQty | int | Units produced per recipe execution |
| CraftingTime | int | Cycle duration in seconds |
| BaseSpeed | double | Machine performance multiplier (0.67-1.5) |
| 60 | constant | Minutes normalization factor

**Throughput Examples:**

**Advanced Circuit (target: 10 units/min):**
- Input: output=1, craftingTime=6s, speed=1.25
- Calculation: (1/6) × 1.25 × 60 = 12.5 units/min
- Adjusted target: 10 units/min

**Express Splitter (target: 2.5 units/min):**
- Input: output=2, craftingTime=15s, speed=1.25
- Calculation: (2/15) × 1.25 × 60 = 20 units/min
- Adjusted target: 2.5 units/min

---

## Component Architecture

### 1. Model Layer (`Models/`)

#### Machine Model
```csharp
namespace FactorioModeler.Engine.Models
{
    [Jacobian]
    public class Machine
    {
        public string Id { get; set; }                    // Unique ID (e.g., "assembling-machine-1")
        public string Name { get; set; }                   // Display name
        public string Type { get; set; }                   // Machine type ID
        public string Emoji { get; set; }                  // Unicode display
        public double MinSpeed { get; set; }               // Speed range minimum (0.0)
        public double MaxSpeed { get; set; }               // Speed range maximum (10.0)
        public string Category { get; set; }               // Category slug
        public int MaxOutputs { get; set; }                // Concurrent slots
        public string CategoryColor { get; set; }          // Hex color
        public string CategoryName { get; set; }           // Display category
        public double BaseSpeed { get; set; }              // Multiplier (0.67, 1.0, 1.5)
    }

    public enum MachineCategory
    {
        Industrial,    // Production: assembling-machine, furnace
        Storage,       // Buffer/storage: buffer
        Belt,          // Transport: belt
        Circuit,       // Network: circuit
        Module,        // Crafting modules
        Tank           // Liquids: tank
    }
}
```

**Machine Categories:**
| Category | Examples | Max Outputs |
|----------|----------|-------------|
| Industrial | assembling-machine-1, furnace, circuit | 10 |
| Storage | buffer | 1 |
| Belt | belt | 1 |
| Circuit | circuit | 1 |
| Module | crafting-module | 1 |
| Tank | tank | 1 |

#### Recipe Model
```csharp
namespace FactorioModeler.Engine.Models
{
    [Serializable]
    public class Recipe
    {
        public string Id { get; set; }                    // Unique ID
        public string MachineType { get; set; }            // Required machine
        public string RecipeName { get; set; }             // Display name
        public string OutputItemId { get; set; }           // Produced item
        public int OutputQty { get; set; }                 // Units per cycle
        public int CraftingTime { get; set; }              // Cycle duration
        public List<ResourceRequirement> RequiredResources { get; set; }
        
        public double CalculateThroughput(double baseSpeed = 1.0)
        {
            return (OutputQty * 1.0 / CraftingTime) * baseSpeed * 60;
        }
    }

    public class ResourceRequirement
    {
        public string ItemId { get; set; }         // Resource ID
        public int Amount { get; set; }            // Quantity per cycle
        public int Minutely { get; set; }          // Per-minute rate
    }
}
```

---

### 2. Factory Layer

#### RecipeFactory
**Purpose:** Deserializes JSON production data into `Recipe` objects

**Key Methods:**
```csharp
public static List<Recipe> DeserializeRecipes(string json)
{
    // Extracts recipes, machines, speedTiers from JSON
    // Returns collection ready for throughput calculation
}

public class RecipeNode
{
    // Graph representation for production chain analysis
}
```

#### MachineFactory
**Purpose:** Serializes `Machine` objects to JSON

**Key Methods:**
```csharp
public static string SerializeToJson(List<Machine> machines)
public static string ToJson(List<Machine> machines)
```

**String Escaping:**
```csharp
private static string Escape(string? text) =>
    text?.Replace("\\", "\\\\").Replace(@""", @"""""""""")));
```

---

### 3. Calculation Layer

#### ThroughputEngine
**Purpose:** Core computational engine for throughput calculations

**Public API:**
```csharp
public class ThroughputCalculator
{
    // Single calculation
    public static double CalculateThroughput(
        double outputQty,           // Units per batch
        int craftingTime,           // Cycle duration (seconds)
        double baseSpeed = 1.0      // Machine speed multiplier
    )
{
        return (outputQty / craftingTime) * baseSpeed * 60;
    }

    // Recipe-based calculation with speed tiers
    public static double CalculateWithSpeedTiers(
        Recipe recipe,
        Dictionary<string, Machine> machines,
        Dictionary<string, double> speedTiers
    )
{}

    // Maximum throughput
    public static double CalculateMaxThroughput(
        double outputQty,
        int craftingTime,
        double maxSpeed,
        double baseSpeed = 1.0
    )
{}

    // Aggregate production line
    public static double AggregateThroughput(
        List<Recipe> recipes,
        Dictionary<string, Machine> machines,
        Dictionary<string, double> speedTiers
    )
{}
}
```

**Speed Tier Configuration:**
| Machine Type | Base Speed | Description |
|--------------|------------|-------------|
| assembling-machine-1 | 0.67 | Basic assembling machine |
| assembling-machine-2 | 1.0 | Standard assembling machine |
| assembling-machine-3 | 1.5 | Advanced assembling machine |

**Module Boosts:**
| Module Type | Speed Multiplier |
|-------------|------------------|
| Crafting Module I | 1.2× (20% boost) |
| Crafting Module II | 1.4× (40% boost) |
| Crafting Module III | 1.6× (60% boost) |

---

### 4. Validation Layer

#### Data Schema Constraints

**Recipe Validations:**
- `recipeName` must be unique
- `outputItemId` must be valid Factorio item ID
- `requiredResources` must be non-empty array
- `craftingTime` must be positive integer
- `machineType` must exist in machines array

**Machine Validations:**
- `maxSpeed` ≥ `minSpeed`
- `maxOutputs` must be positive integer (≥1)
- `categoryColor` must be valid hex

**Consistency Checks:**
- `speedTiers` keys must match machine `type` values
- Machine `id` must align with recipe `machineType`

---

## Machine Registry

### Complete Machine Types

| ID | Name | Type | Min/Max Speed | Outputs |
|----|------|------|---------------|---------|
| assembling-machine-1 | Assembling Machine L1 | industrial | 0-1 | 10 |
| assembling-machine-2 | Assembling Machine L2 | industrial | 0-10 | 10 |
| furnace | Furnace | industrial | 0-1 | 10 |
| belt | Conveyor Belt | belt | 0-1 | 1 |
| buffer | Buffer | storage | 0-1 | 1 |
| circuit | Circuit Network | circuit | 0-1 | 1 |
| crafting-module | Crafting Module | module | 0-1 | 1 |
| tank | Liquid Tank | tank | 0-1 | 1 |

### Speed Tier Defaults

```csharp
// Default speed tier lookup
private static Dictionary<string, double> DefaultSpeedTiers = new()
{
    { "assembling-machine-1", 0.67 },
    { "assembling-machine-2", 1.0 },
    { "assembling-machine-3", 1.5 }
};
```

---

## Sample Recipes

### Advanced Circuit (adv-circuit-9x)
```
{
  "id": "advanced-circuit-9x",
  "machineType": "assembling-machine-2",
  "recipeName": "Advanced Circuit",
  "outputItemId": "advanced-circuit",
  "outputQty": 10,
  "craftingTime": 6,
  "requiredResources": [
    { "itemId": "iron-plate", "amount": 200, "minutely": 1000 },
    { "itemId": "copper-cable", "amount": 10, "minutely": 50 },
    { "itemId": "steel-plate", "amount": 50, "minutely": 3000 },
    { "itemId": "copper-plate", "amount": 100, "minutely": 5000 },
    { "itemId": "copper-cable-m", "amount": 14, "minutely": 100 }
  ]
}
```
**Calculated Throughput:** 10 units/min (target)

### Express Splitter Basic
```
{
  "id": "express-splitter-basic",
  "machineType": "assembling-machine-2",
  "recipeName": "Express Splitter",
  "outputItemId": "express-splitter",
  "outputQty": 2,
  "craftingTime": 18,
  "requiredResources": [
    { "itemId": "advanced-circuit", "amount": 1500, "minutely": 100000 },
    { "itemId": "copper-plate", "amount": 750, "minutely": 50000 },
    { "itemId": "iron-plate", "amount": 50, "minutely": 3000 }
  ]
}
```
**Calculated Throughput:** 2.5 units/min (target)

---

## File Structure

```
FactorioModeler.Engine/
├── BottleneckAnalyzer.cs      # Critical path identification
├── CircuitNetworkConfig.cs    # Circuit network management
├── LayoutOptimizer.cs         # Production layout optimization
├── MachineFactory.cs          # Machine serialization
├── Machine.cs                 # Machine data model
├── Models/                    # Core data models
│   ├── Machine.cs             # Machine definition with Jacobian attr
│   └── Recipe.cs              # Recipe definition with throughput calc
├── RecipeFactory.cs           # Recipe deserialization
├── ResourceBalancer.cs        # Resource analysis
├── ThroughputEngine.cs        # Throughput calculations
└── README.md                  # This documentation
```

---

## Testing Strategy

### Unit Tests

1. **Throughput Calculation Accuracy**
```csharp
[Fact]
public void Simple_Throughput_Returns_Correct_Value()
{
    double result = ThroughputEngine.CalculateThroughput(10, 15, 1.0);
    Assert.Equal(40.0, result);
}
```

2. **Speed Tier Application**
```csharp
[Fact]
public void SpeedTiers_Multiplier_Applied_Correctly()
{
    var recipe = new Recipe { OutputQty = 1, CraftingTime = 15 };
    double result = ThroughputEngine.CalculateWithSpeedTiers(
        recipe, new Machine { Type = "assembling-machine-2" },
        new Dictionary<string, double> {{ "assembling-machine-2", 1.25 }}
    );
    Assert.Equal(48.0, result, 0.01);
}
```

### Acceptance Tests

1. **Advanced Circuit Test**
   - Expected: 10 units/min
   - Input: AM2, recipe=(1 unit, 6s)
   - Result: Pass

2. **Express Splitter Test**
   - Expected: 2.5 units/min
   - Input: AM2, recipe=(2 units, 15s)
   - Result: Pass

---

## Performance Considerations

1. **O(1) Lookups**: Dictionary-based machine lookup
2. **Lazy Loading**: Objects instantiated on demand
3. **Pure Functions**: Deterministic inputs → deterministic outputs
4. **Thread-Safe**: No shared mutable state in calculations

---

## Error Handling

| Exception | Trigger Condition | Resolution |
|-----------|-------------------|------------|
| `JsonException` | Invalid JSON input | Validate JSON schema before deserialization |
| `NullReferenceException` | Null input | Add null guards before operation |
| `InvalidOperationException` | Invalid speed tier | Apply default 1.0 multiplier |

---

## Dependencies
- `System.Text.Json` - JSON deserialization
- `System.Collections.Generic` - Dictionary/List operations
- `System.IO` - File operations

---

## Version History

| Version | Date | Changes |
|---------|------|--------|
| 1.0 | 2024-01-15 | Initial release with throughput formula |
| 1.1 | Current | Speed tier scaling and machine categories |

---

## Contact

Questions about this engine subsystem should be directed to the Factorio Modeler development team.

---

*Factorio Modeler.Engine Technical Architecture Documentation*
*Version: 1.1 | .NET 8 LTS | Core Computational Backend*
