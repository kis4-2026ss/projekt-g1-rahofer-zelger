# Machines Model

This directory contains the Machine data model used for modeling Factorio production machinery.

## Machine Class

The `Machine` class represents an industrial machine with capabilities for speed tiers and output configuration.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique machine identifier |
| `Name` | string | Human-readable display name |
| `Type` | string | Machine type ID (e.g., "assembling-machine-1") |
| `Emoji` | string | Icon representation |
| `MinSpeed` | int | Minimum operational speed (0) |
| `MaxSpeed` | int | Maximum operational speed (10) |
| `Category` | string | Machine category classification |
| `MaxOutputs` | int | Maximum concurrent output slots |
| `CategoryColor` | string | Display color code |
| `CategoryName` | string | Display category label |
| `BaseSpeed` | double | Base speed multiplier |
| `IsOperational` | bool | Operational status |

### Methods

```csharp
public double GetCurrentSpeed() => BaseSpeed * (ActiveOutputs / (double)MaxOutputs);
public bool Validate() => string.IsNullOrWhiteSpace(Id) ? false : BaseSpeed > 0;
public bool TryInstallModule(Module module) => Modules != null && Modules.Count < MaxOutputs ? Modules.Add(module) : false;
```

### Module Support

The `Module` class represents optional factory modules that modify behavior:

| Module Type | Effect |
|-------------|--------|
| Crafting I | +20% speed |
| Crafting II | +40% speed |
| Crafting III | +60% speed |

---

*Factorio Modeler - Machines Model*
