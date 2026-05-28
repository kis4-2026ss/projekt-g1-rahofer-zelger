# Simulation Model

This directory contains simulation state models used for temporal production analysis.

## MachineState Class

The `MachineState` class represents the runtime state of individual machines during simulation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Machine identifier |
| `IsActive` | bool | Active/inactive status |
| `Efficiency` | double | Current efficiency (0.0 - 1.0) |
| `Error` | string | Error/warning message |
| `Outputs` | int | Current output count |

### Methods

```csharp
public string Status =
    IsActive ? $"Running (Efficiency: {Efficiency:F0}%)" : "Stopped";
```

## BufferState Class

The `BufferState` class represents material buffer/inventory states.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemId` | string | Material item ID |
| `Level` | int | Current inventory count |
| `Capacity` | int | Maximum buffer capacity |

### Methods

```csharp
public double Utilization =
    Capacity > 0 ? (Level / (double)Capacity) * 100.0 : 0.0;
```

## BeltState Class

The `BeltState` class represents conveyor belt states.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Belt identifier |
| `Materials` | List | Materials on belt |
| `IsActive` | bool | Belt active status |
| `Speed` | int | Belt speed factor |

### Methods

```csharp
public List<Material> Materials;
```

## Material Class

The `Material` class represents items currently on conveyor belts.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemId` | string | Material item ID |
| `Quantity` | int | Units on this belt segment |

## SimulationState Class

The `SimulationState` class represents the complete factory-wide simulation state.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TimeStep` | int | Current simulation step |
| `ElapsedTime` | double | Total elapsed time in seconds |
| `Efficiency` | double | Overall factory efficiency |
| `Belts` | Dictionary | Current belt states |
| `Buffers` | Dictionary | Current buffer states |
| `Machines` | Dictionary | Current machine states |
| `Resources` | Dictionary | Resource stock levels |
| `Queue` | List | Production queue items |

### Methods

```csharp
public bool Advance() =
    { TimeStep++; ElapsedTime++; return true; }

public string ToString() =
    $"SimulationState[Step={TimeStep}, Efficiency={Efficiency:F0}%, Time={ElapsedTime}s]";
```

### Simulation Initialization

```csharp
public SimulationState()
{
    Belts = new Dictionary<string, BeltState>();
    Buffers = new Dictionary<string, BufferState>();
    Machines = new Dictionary<string, MachineState>();
    Resources = new Dictionary<string, int>();
    Queue = new List<string>();
}
```

---

*Factorio Modeler - Simulation Model*
