# Speed Model

This directory contains speed tier configuration models used for machine performance scaling.

## SpeedTier Class

The `SpeedTier` class represents speed multiplier configurations for specific machine types.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `MachineType` | string | Machine type identifier |
| `Speed` | double | Performance multiplier |

### Constructors

```csharp
public SpeedTier(string machineType, double speed);
public SpeedTier();  // Default initializer
```

## SpeedTierCollection Class

The `SpeedTierCollection` class manages speed tier mappings for all machine types.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Tiers` | Dictionary | Speed tier entries per machine type |
| `BaseSpeed` | double | Default base speed (1.0) |

### Methods

```csharp
public void Add(string machineType, double speed);
public SpeedTier GetSpeedTier(string machineType);
```

### Speed Tier Defaults

| Machine Type | Base Speed |
|--------------|------------|
| Assembling Machine 1 | 0.67x |
| Assembling Machine 2 | 1.0x |
| Assembling Machine 3 | 1.5x |

---

*Factorio Modeler - Speed Model*
