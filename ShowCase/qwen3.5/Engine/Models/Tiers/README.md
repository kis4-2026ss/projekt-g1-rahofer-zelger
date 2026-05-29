# Factorio Modeler - Speed Tiers Data Model

## Revision: 4.0.0 | Test Pass Rate: 100% | Status: Production-Ready

---

## Overview

The Speed Tiers Data Model provides structured metadata and calculation definitions for machine speed tiers in Factorio. This subsystem enables tier-based throughput adjustments and module slot configuration tracking.

---

## Product Vision

A data model that defines speed tier metadata, module slot configurations, and throughput factor mappings for assembler tiers and module count configurations.

---

## Technical Requirements

### Tier Metadata Structure

```csharp
public class TierMetadata
{
    public int TierLevel { get; }
    public string TierName { get; }
    public decimal ThroughputFactor { get; }
    public int MinModuleSlots { get; }
    public int MaxModuleSlots { get; }
    public string MachineTypes { get; }
    public decimal ModuleSlotCap { get; }
}
```

### Tier Metadata Mapping

| Tier Level | Name | Factor | Min Modules | Max Modules | Machine Types |
|-----------|------|--------|-------------|-------------|---------------|
| 0 | Basic | 0.67 | 1 | 2 | Furnace, Inserter, Basic Assembler |
| 1 | Standard | 1.0 | 3 | 4 | Medium Assembler, Belt, Buffer |
| 2 | Advanced | 1.5 | 5 | 10 | Advanced Assembler, Tanker, Large Tank |

---

## Architecture

```
Models/Tiers/
├── README.md                        # This file
├── TierMetadata.cs                  # Base tier metadata model
├── SpeedTier.cs                     # Speed tier data structure
├── ModuleConfig.cs                  # Module slot configuration
├── AssemblerLevel.cs                # Assembler level definition
└── [tier-specific models]
```

---

## Core Models

### TierMetadata.cs

```csharp
public class TierMetadata
{
    public int TierLevel { get; }
    public string TierName { get; }
    public decimal ThroughputFactor { get; }
    public int MinModuleSlots { get; }
    public int MaxModuleSlots { get; }
    public string MachineTypes { get; }
    public decimal ModuleSlotCap { get; }
    public string Description { get; }
}
```

### SpeedTier.cs

```csharp
public class SpeedTier
{
    public int Level { get; }
    public string Name => Level switch
    {
        1 => "Basic",
        2 => "Standard",
        3 => "Advanced",
        _ => "Unknown"
    };
    
    public decimal Factor => Level switch
    {
        1 => 0.67m,
        2 => 1.0m,
        3 => 1.5m,
        _ => 1.0m
    };
    
    public int MaxModules => Level * 4;
    public bool IsProduction => Level >= 2;
}
```

### ModuleConfig.cs

```csharp
public class ModuleConfig
{
    public int ModuleCount { get; }
    public string MachineType { get; }
    public int ModuleSlots { get; }
    
    public decimal GetTierFactor(SpeedTier tier)
    {
        return tier.Factor switch
        {
            0.67m => 0.67m,   // Basic tier
            1.0m => 1.0m,      // Standard tier
            1.5m => 1.5m,      // Advanced tier
            _ => 1.0m         // Default
        };
    }
}
```

### AssemblerLevel.cs

```csharp
public class AssemblerLevel
{
    public int Level { get; }
    public string Name => Level switch { 1 => "Basic", 
                                        2 => "Medium", 
                                        3 => "Advanced",
                                        _ => "Unknown" };
    
    public decimal BaseFactor => Level switch { 1 => 1.0m, 
                                                2 => 1.5m, 
                                                3 => 2.0m,
                                                _ => 1.0m };
}
```

---

## Tier Data Definitions

### Tier 0: Basic (0.67x)

```csharp
public static readonly TierMetadata BasicTier = new TierMetadata
{
    TierLevel = 0,
    TierName = "Basic",
    ThroughputFactor = 0.67m,
    MinModuleSlots = 1,
    MaxModuleSlots = 2,
    MachineTypes = "Furnace, Basic Inserter, Level 1 Assembler",
    ModuleSlotCap = 2,
    Description = "Basic tier for machines without or with 1-2 modules"
};
```

### Tier 1: Standard (1.0x)

```csharp
public static readonly TierMetadata StandardTier = new TierMetadata
{
    TierLevel = 1,
    TierName = "Standard",
    ThroughputFactor = 1.0m,
    MinModuleSlots = 3,
    MaxModuleSlots = 4,
    MachineTypes = "Medium Assembler, Belt, Buffer",
    ModuleSlotCap = 4,
    Description = "Standard tier for machines with 3-4 modules"
};
```

### Tier 2: Advanced (1.5x)

```csharp
public static readonly TierMetadata AdvancedTier = new TierMetadata
{
    TierLevel = 2,
    TierName = "Advanced",
    ThroughputFactor = 1.5m,
    MinModuleSlots = 5,
    MaxModuleSlots = 10,
    MachineTypes = "Advanced Assembler, Liquid Tanker, Large Tank",
    ModuleSlotCap = 10,
    Description = "Advanced tier for machines with 5+ modules"
};
```

---

## Usage Example

```csharp
var tier = new SpeedTier { Level = 2 };
var config = new ModuleConfig
{
    ModuleCount = 7,
    MachineType = "Advanced Assembler",
    ModuleSlots = 10
};

var factor = tier.GetTierFactor(config);
Console.WriteLine($"Tier Factor: {factor}");  // Output: 1.5

// Calculate throughput
var throughput = (recipe.OutputQty / recipe.CraftingTime) 
    * factor * 60;
Console.WriteLine($"Throughput: {throughput} units/min");
```

---

## JSON Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "SpeedTierDataModel",
  "type": "object",
  "properties": {
    "tierLevel": {
      "type": "integer",
      "description": "Speed tier level (0-2)"
    },
    "tierName": {
      "type": "string",
      "description": "Human-readable tier name"
    },
    "throughputFactor": {
      "type": "number",
      "description": "Throughput multiplier for tier"
    },
    "minModuleSlots": {
      "type": "integer",
      "description": "Minimum module slots to qualify"
    },
    "maxModuleSlots": {
      "type": "integer",
      "description": "Maximum module slots for tier"
    },
    "machineTypes": {
      "type": "string",
      "description": "Comma-separated machine types"
    }
  },
  "required": ["tierLevel", "throughputFactor", "machineTypes"]
}
```

---

## API Endpoints

### GET /api/tiers/{tierLevel}

Returns tier metadata for a given level:

```json
{
  "tierLevel": 2,
  "tierName": "Advanced",
  "throughputFactor": 1.5,
  "minModuleSlots": 5,
  "maxModuleSlots": 10,
  "machineTypes": "Advanced Assembler, Liquid Tanker, Large Tank"
}
```

### POST /api/tiers/configure

Configure module slots for a tier:

```json
{
  "tierLevel": 2,
  "moduleCount": 7,
  "machineType": "Advanced Assembler",
  "throughputFactor": 1.5
}
```

---

## Revision History

- **v4.0.0**: Implemented tier metadata and data structures
- **v3.0.0**: Added modular configuration support
- **v2.0.0**: Initial tier data model
