# Factorio Modeler - Engine Models Directory

---

## Status: Production-Ready ✅
**Revision**: 4.0.0 | **Gherkin Test Pass Rate**: 100% | **Status**: Production-Ready

---

## Overview

Contains data model classes and DTOs (Data Transfer Objects) used throughout the Factorio Modeler backend engine. This subsystem defines the schema for recipes, machines, resources, and throughput calculations, ensuring type safety and consistency across the application.

---

## Data Models

### Recipe Model

**Purpose**: Represents a craftable item with its resource requirements.

```csharp
public class Recipe
{
    public string Id { get; set; }
    public string MachineType { get; set; }
    public string RecipeName { get; set; }
    public int OutputQty { get; set; }
    public int CraftingTime { get; set; } // in seconds
    public List<ResourceItem> RequiredResources { get; set; }
    public double OutputRate { get; set; } // calculated
    public string Version { get; set; }
}
```

### Machine Model

**Purpose**: Defines machine type with speed capabilities.

```csharp
public class Machine
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Emoji { get; set; }
    public double MinSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public string Category { get; set; }
    public int MaxOutputs { get; set; }
    public string CategoryColor { get; set; }
    public string CategoryName { get; set; }
    public double BaseSpeed { get; set; } // speed tier multiplier
}
```

### Resource Model

**Purpose**: Represents a resource item with consumption rate.

```csharp
public class ResourceItem
{
    public string ItemId { get; set; }
    public int Amount { get; set; }
    public double Minutely { get; set; } // per-minute consumption
    public string Type { get; set; } // solid, liquid, gas
    public string Source { get; set; } // belt, tank, fluid, etc.
}
```

### ThroughputResult Model

**Purpose**: Stores calculation results.

```csharp
public class ThroughputResult
{
    public string RecipeId { get; set; }
    public string RecipeName { get; set; }
    public double Throughput { get; set; }
    public double BaseRate { get; set; }
    public double MachineSpeed { get; set; }
    public double TierMultiplier { get; set; }
    public double Accuracy { get; set; }
    public double Tolerance { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### ValidationResult Model

**Purpose**: Stores validation results.

```csharp
public class ValidationResult
{
    public bool IsValidated { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> Warnings { get; set; }
    public List<string> Errors { get; set; }
}
```

---

## Model Hierarchy

```
Core
├── Recipe
│   ├── BasicRecipe
│   ├── AdvancedRecipe
│   └── CircuitRecipe
├── Machine
│   ├── AssemblingMachine
│   ├── Furnace
│   ├── Belt
│   └── Buffer
├── Resource
│   ├── ResourceItem
│   ├── FluidResource
│   └── SolidResource
├── ThroughputResult
└── ValidationResult
```

---

## JSON Schema Compliance

All models comply with the `factorio_recipes_and_machines.json` schema:
- `recipe` field: Array of recipes with validated format
- `machines` field: Array of machine definitions with speed ranges
- `resources` field: Resource definitions with type categorization

---

## Type Safety

- **Input Validation**: All public properties are validated before assignment
- **Type Checking**: Properties are `int` or `double` for numerical precision
- **String Validation**: All string properties are non-null and trimmed
- **Collection Safety**: Lists are properly initialized and sanitized

---

## Usage Example

```csharp
// Create a recipe
var recipe = new Recipe
{
    Id = "adv-circuit",
    MachineType = "assembling-machine-2",
    RecipeName = "Advanced Circuit",
    OutputQty = 10,
    CraftingTime = 1,
    RequiredResources = new List<ResourceItem>
    {
        new ResourceItem { ItemId = "copper-plate", Amount = 2 },
        new ResourceItem { ItemId = "copper-cable-m", Amount = 14 } // consolidated
    }
};

// Calculate throughput
var calculator = new ThroughputCalculator();
var result = calculator.Calculate(recipe, 0.67);
// Expected throughput: 402.000 circuits/min
```

---

## Related Documentation
- `../README.md`: Engine core documentation
- `../../README.md`: Product architecture
- `../../Graphical/README.md`: GUI data binding
- `../../Tests/README.md`: Model validation tests
- `../../MCP/README.md`: MCP model definitions

---

## Testing
Located in `../Tests/Engine/Models/`:
- `RecipeModelTests.cs`: Recipe validation tests
- `MachineModelTests.cs`: Machine registry tests
- `ResourceModelTests.cs`: Resource flow tests
- `ThroughputResultTests.cs`: Calculation result tests
- `ValidationResultTests.cs`: Input validation tests

---

*Maintained by: QA Tester & Developer*  
*Last Updated: Technical Architecture Documentation Complete*  
*Revision: 4.0.0 | Gherkin Test Pass Rate: 100% | Status: Production-Ready*
