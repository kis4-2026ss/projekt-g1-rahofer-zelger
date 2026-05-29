# Products Specification

## Overview

This document defines the Products Specification for the Factorio Modeler system. Products represent end-result items or intermediate goods manufactured by the factory simulation.

## Purpose

The Products subsystem provides:
- Type-safe product definitions for the simulation
- Resource and machine consumption tracking
- Product-to-recipe mapping
- Integration with Machines and Recipes subsystems

## Product Model Architecture

### Core Product Properties

Products are defined with the following key attributes:

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique identifier for the product |
| `ItemId` | string | Factorio item ID reference |
| `Name` | string | Human-readable product name |
| `RecipeId` | string | References the Recipe that produces this product |
| `Description` | string | Product description and usage context |
| `Icon` | string | Emoji/icon representation |

### Product Lifecycle

1. **Definition**: Products are defined by their recipes and machine requirements
2. **Resource Mapping**: Each product maps to RequiredResource consumption
3. **Production Rate**: Products specify output rate based on machine speed tiers
4. **Validation**: Products validate against existing recipes and machine types

## Relationship with Other Models

### Recipes

```csharp
Recipe.RecipeId → Products.Product.RecipeId
Recipe.OutputItemId = Products.Product.ItemId
```

### Machines

- Products specify the machine type used for their production
- Machine speed tiers affect product output rate
- Machine categories must match product complexity level

### Resources

- Products define end-products of the resource chain
- Resources consumed by machines are tracked via Recipe.RequiredResources

## Usage Example

### Simple Product Definition

```csharp
public class Product
{
    public string Id { get; set; }
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string RecipeId { get; set; }
    public int OutputRate { get; set; }
    public Dictionary<int, MachineSpeedTier> SpeedTiers { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
}
```

## Data Flow

```
Recipe Definition
        ↓
    Product Creation
        ↓
Machine Selection
        ↓
Resource Consumption
        ↓
Simulation Output
```

## Categories

Products are categorized for easier organization:

- **Industrial** - Machines with high throughput and resource consumption
- **Transport** - Belt-connected items and carriers
- **Storage** - Buffers and intermediate goods
- **Power** - Energy generation and storage
- **Crafting** - Basic manufactured items

## Naming Conventions

- **Class**: `Product` (PascalCase)
- **Properties**: `Id`, `ItemId`, `OutputRate` (PascalCase)
- **Constants**: `PRODUCT_CATEGORIES` (PascalCase)

## Best Practices

1. **Validation**: Always validate product references to existing recipes
2. **Documentation**: Describe each product's purpose and usage
3. **Performance**: Consider memory impact for large-scale simulations
4. **Serialization**: Ensure JSON compatibility with Factorio item IDs

## Versioning

- Schema version: `1.0`
- Breaking changes: Version bump required

---
*Factorio Modeler - Products Specification Documentation*
*Version: 1.0 | Generated: 2024-01-15T10:00:00Z*
*Framework: .NET 8.0 C#*
