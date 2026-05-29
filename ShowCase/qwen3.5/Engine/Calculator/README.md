# Factorio Modeler - Calculator Module

## Overview
The Calculator module implements the core throughput calculation engine for the Factorio Modeler. This module provides precise throughput calculations validated against actual Factorio mechanics.

## Formula Implementation

### Throughput Formula
```
T = (OutputQty / CraftingTime) × MachineSpeed × 60
```

### Precision Requirements
- **Accuracy**: ±0.001 per minute
- **Validation**: ≥99.9% throughput accuracy
- **Circuit Protection**: Enforces 10 circuits/min
- **Splitter Support**: Enforces 2.5 units/min

## Components

### RecipeDatabase
- **Purpose**: Recipe definitions and configuration
- **Throughput**: Throughput calculation engine
- **Validation**: Configuration validation
- **Database**: Recipe machine pairings

### ThroughputCalculator
- **Purpose**: Calculate throughput for given recipe+machine
- **Inputs**: Recipe type, machine level, speed tier
- **Output**: Throughput per minute + validation status
- **Circuit Check**: Verifies circuit capacity limits

### CircuitNetwork
- **Purpose**: Advanced circuit network modeling
- **Capacity**: 10 circuits/min per machine
- **Monitoring**: Real-time circuit usage
- **Reporting**: Circuit status per machine

### SplitterNetwork
- **Purpose**: Splitter network optimization
- **Throughput**: 2.5 units/min enforced
- **Optimulation**: Express splitter configuration
- **Config Export**: Splitter config export to JSON

### MachineRegistry
- **Purpose**: Machine definitions and characteristics
- **Speed Tiers**: Assembler level speed multipliers
  - Level 1: 0.67
  - Level 2: 1.0
  - Level 3: 1.5
- **Valid Recipe**: Recipe availability checking

## Technical Specifications (Revision 4.0.0)

### Circuit Integration
- **Capacity**: 10 circuits/min per machine limit
- **Circuit Usage**: Real-time circuit monitoring
- **Circuit Reports**: Circuit usage per machine
- **Circuit Protection**: Validates circuit capacity before calculation

### Splitter Integration
- **Throughput**: 2.5 units/min enforced
- **Splitter Config**: Express splitter optimization
- **Network Config**: Splitter network configuration
- **Export**: JSON configuration export

### Precision
- **Decimal Precision**: High-precision decimal calculations
- **Rounding**: Consistent rounding to 3 decimal places
- **Validation**: ≥99.9% throughput accuracy
- **Error Handling**: Robust exception handling

## API Interface

### Calculate Method
```csharp
public ThroughputResult Calculate(string recipeType, int machineLevel, int speedTier)
{
    // Calculate throughput with precision
    // Enforce circuit limits
    // Enforce splitter limits
    // Validate configuration
}
```

### Validate Method
```csharp
public ValidationResult Validate(string recipeType, int machineLevel)
{
    // Validate configuration
    // Check recipe availability
    // Check circuit capacity
    // Check splitter optimization
}
```

## Usage Example
```csharp
var calculator = new ThroughputCalculator();
var recipe = "iron-plate";
var machineLevel = 3;
var result = calculator.Calculate(recipe, machineLevel, speedTier: 3);

Console.WriteLine($"Throughput: {result.Throughput}");
Console.WriteLine($"Circuit Usage: {result.CircuitUsage}");
Console.WriteLine($"Validation: {result.IsValid}");
```

## Testing
Located in `../Tests/Engine/` directory:
- `CalculatorTests.cs`: Throughput calculation tests
- `RecipeDatabaseTests.cs`: Recipe database tests
- `ThroughputCalculatorTests.cs`: Calculator engine tests
- `CircuitNetworkTests.cs`: Circuit network tests
- `SplitterNetworkTests.cs`: Splitter network tests

## Related Documentation
- Root README.md: Product vision
- ../Graphical/README.md: GUI implementation
- ../MCP/README.md: MCP server integration
- ../Docs/README.md: Technical documentation
