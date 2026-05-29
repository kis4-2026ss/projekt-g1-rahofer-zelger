# Recipes Model

This directory contains the Recipe data model used for modeling crafting recipes in Factorio production chains.

## Recipe Class

The `Recipe` class represents crafting operations with resource requirements.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique recipe identifier |
| `MachineType` | string | Required machine type ID |
| `RecipeName` | string | Human-readable display name |
| `OutputItemId` | string | Produced item ID |
| `OutputQty` | int | Units produced per cycle |
| `CraftingTime` | int | Cycle duration in seconds |
| `OutputRate` | int | Pre-calculated throughput |
| `RequiredResources` | List | Required input resources |

### Methods

```csharp
public Dictionary<string, int> ComputeTotalResourceConsumption() =
    foreach (var resource in RequiredResources)
        consumption[resource.ItemId] += resource.Amount;

public double CalculateEffectiveThroughput(double machineSpeed) =
    (OutputQty / (double)CraftingTime) * machineSpeed;
```

## RequiredResource Class

The `RequiredResource` class represents individual material requirements.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemId` | string | Resource item ID |
| `Amount` | int | Quantity per cycle |
| `Minutely` | int | Per-minute throughput rate |

### Methods

```csharp
public bool Equals(RequiredResource other) =
    other?.ItemId == ItemId && other?.Amount == Amount;
```n
---

*Factorio Modeler - Recipes Model*
