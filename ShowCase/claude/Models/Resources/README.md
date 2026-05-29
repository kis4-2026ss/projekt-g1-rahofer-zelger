# Resources Model

This directory contains the resource data models used for tracking material consumption and inventory.

## RequiredResource Class

The `RequiredResource` class represents materials required for recipe execution.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemId` | string | Factorio item ID reference |
| `Amount` | int | Quantity needed per craft |
| `Minutely` | int | Continuous throughput rate |

### Constructors

```csharp
public RequiredResource(string itemId, int amount, int minutely);
public RequiredResource();  // Default initializer
```

### Validation

| Rule | Description |
|------|-------------|
| ItemId | Must be non-null, non-empty |
| Amount | Must be positive integer |
| Minutely | Must be non-negative |

---

*Factorio Modeler - Resources Model*
