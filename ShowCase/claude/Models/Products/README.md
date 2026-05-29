# Products Specification

This directory contains the Product model class used for modeling manufactured items in Factorio production chains.

## Product Class

The `Product` class represents end-items or intermediate goods created by recipes.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique product identifier |
| `ItemId` | string | Factorio item ID reference |
| `Name` | string | Human-readable display name |
| `RecipeId` | string | Recipe ID that produces this product |
| `Description` | string | Usage context and details |
| `Icon` | string | Emoji representation |
| `OutputRate` | double | Throughput per speed tier |
| `MachineType` | string | Production machine type |
| `Category` | string | Category for organization |
| `CategoryEmoji` | string | Category emoji indicator |
| `SpeedTiers` | Dictionary | Speed multiplier configuration |
| `Consumable` | bool | Whether product is consumable |
| `Quantity` | int | Units per crafting cycle |

### Methods

```csharp
public double CalculateEffectiveThroughput(double machineSpeed) =
    SpeedTiers.TryGetValue(machineSpeed.ToString(), out var tier)
    ? OutputRate * tier / CraftingTimeSeconds()
    : OutputRate / CraftingTimeSeconds();

public Machine GetMachineInfo(string machineId) =
    ProductManager.Instance?.GetMachineById(machineType) ?? null;

public double CalculateProductThroughput(string productId, string machineId) =
    (Quantity / CraftingTimeSeconds()) * GetMachineSpeed() * 60;
```

### ProductManager Singleton

```csharp
public class ProductManager
{
    public static ProductManager Instance => _instance ?? (_instance = new ProductManager());
    
    public void RegisterProduct(Product product);
    public Product GetProductById(string id);
    public Dictionary<string, Product> GetAllProducts();
    public void RegisterMachine(Machine machine);
    public Machine GetMachineById(string id);
    public void RegisterRecipe(Recipe recipe);
    public Recipe GetRecipeById(string id);
}
```

### Product Categories

```csharp
public static class ProductCategories
{
    public const string Industrial = "Industrial";
    public const string Transport = "Transport";
    public const string Storage = "Storage";
    public const string Power = "Power";
}
```

| Category | Color |
|----------|-------|
| Industrial | #e74c3c |
| Transport | #27ae60 |
| Storage | #8e44ad |
| Power | #f39c12 |
| Crafting | #3498db |
---

*Factorio Modeler - Products Specification*
