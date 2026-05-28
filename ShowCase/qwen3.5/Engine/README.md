# Factorio Modeler - Backend Engine Core

## Overview
The Backend Engine Core implements the throughput calculation logic and recipe parsing functionality for the Factorio Modeler. This subsystem provides the mathematical foundation for structural throughput analysis.

## Technical Specifications (Revision 4.0.0)

### Throughput Calculation Engine
- **Formula**: T = (OutputQty / CraftingTime) × MachineSpeed × 60
- **Precision**: ±0.001 per minute (≥99.9% throughput accuracy)
- **Input Validation**: JSON-based recipe data with type safety
- **Machine Speed Range**: Configurable per machine type

### Advanced Circuit Support
- **Circuit Network Capacity**: 10 circuits/minute per machine
- **Circuit Logic Integration**: Support for advanced circuit networks
- **Power Management**: Circuit-protected recipe validation

### Express Splitter Optimization
- **Express Splitter Throughput**: 2.5 units/minute maximum
- **Splitting Logic**: Optimized for complex splitter networks
- **Network Efficiency**: Circuit-aware splitting calculations

### Machine Speed Tiers (Assembler Levels)
| Assembler Level | Speed Multiplier | Description |
|-----------------|------------------|-------------|
| Level 1 | 0.67 | Basic industrial machines |
| Level 2 | 1.0 | Standard production machines |
| Level 3 | 1.5 | High-speed production machines |

## Components

#### Calculator Module
- `ThroughputCalculator.cs`: Core calculation logic
  - **calculateThroughput()**: Main entry point for throughput calculation
  - **validateInputs()**: Input validation and error handling
  - **parseMachineSpeed()**: Dynamic machine speed resolution (supporting 0.67/1.0/1.5 tiers)
  - **precisionMode()**: ±0.001 tolerance enforcement
  - **applyCircuitMultiplier()**: Advanced circuit capacity (10 circuits/min)
  - **optimizeSplitterNetwork()**: Express splitter optimization (2.5 units/min)

#### Recipe Parser
- `RecipeParser.cs`: JSON-based recipe data parsing
  - `parseRecipes()`: Recipe JSON file parsing
  - `validateRecipes()`: Recipe format and content validation
  - `extractResources()`: Resource requirements extraction
  - `buildResourceMap()`: Resource quantity mapping
  - `parseCircuitData()`: Circuit network data parsing
  - `parseSplitterConfig()`: Splitter network configuration

#### Machine Registry
- `MachineRegistry.cs`: Machine type definitions
  - Register machine type IDs (assembling-machine-2, furnace, belt, buffer, etc.)
  - Define speed ranges (minSpeed, maxSpeed)
  - Categorize by type (industrial, belt, storage, circuit, module, tank)
  - Apply assembler level speed tiers (0.67 / 1.0 / 1.5)
  - Define circuit capacity limits (10 circuits/min)
  - Define splitter throughput limits (2.5 units/min)

#### Unit Converter
- `UnitConverter.cs`: Resource flow conversions
  - Convert between per-minute and per-second rates
  - Handle module-based speed multipliers
  - Validate resource quantities
  - Support circuit and splitter conversions

## Mathematical Model

```csharp
public class ThroughputCalculation
{
    public double Calculate(Recipe recipe, double machineSpeed)
    {
        // T = (OutputQty / CraftingTime) × MachineSpeed × 60
        double baseRate = recipe.OutputQty / recipe.CraftingTime;
        
        // Apply assembler level speed tier
        double tieredSpeed = GetSpeedTier(recipe.AssemblerLevel);
        
        // Apply circuit capacity limit
        double circuitAdjustedSpeed = Math.Min(tieredSpeed, 10.0 / recipe.CircuitUsage);
        
        // Apply splitter optimization for relevant machines
        double optimizedSpeed = ApplySplitterOptimization(recipe.SplitterNetwork);
        
        return baseRate * optimizedSpeed * 60;
    }
    
    public double GetAccuracyTolerance() => 0.001; // ±0.001 per minute
    
    public double GetSpeedTier(int assemblerLevel)
    {
        return assemblerLevel switch
        {
            1 => 0.67,
            2 => 1.0,
            3 => 1.5,
            _ => 1.0
        };
    }
}
```

## Performance Requirements
- **Calculation Time**: <1ms per recipe
- **Memory Usage**: <2MB for standard recipe set
- **Concurrency**: Thread-safe for multi-recipe analysis
- **Circuit Response**: <5ms for circuit capacity queries
- **Splitter Optimization**: <3ms for network optimization

## Output Format
- **JSON Output**: `{ "throughput": 10.000, "recipeId": "uuid", "recipeName": "string", "timestamp": "ISO-8601", "circuitStatus": "active", "splitterOptimized": true }`
- **Validation Report**: `{ "isValidated": true, "accuracy": 99.9, "tolerance": 0.001, "circuitCapacity": "ok", "splitterEfficiency": "optimal" }`

## Error Handling
- **Input Validation**: Throws ArgumentException for invalid recipe data
- **Calculation Errors**: Returns negative throughput or throws InvalidOperationException
- **Resource Mismatches**: Logs warnings and continues processing
- **Circuit Overload**: Throws CircuitCapacityException when circuit network exceeded
- **Splitter Limit**: Warns when approaching 2.5 units/min limit

## Dependencies
- `factorio_recipes_and_machines.json`: Primary data source
- `Newtonsoft.Json`: JSON serialization (NuGet package)
- `System.Collections.Generic`: Standard library
- `System.Numerics`: High-precision calculations

## Testing
Located in `../Tests/Engine/` directory:
- ThroughputCalculationTests.cs
- RecipeParserTests.cs
- MachineRegistryTests.cs
- UnitConverterTests.cs
- CircuitNetworkTests.cs
- SplitterOptimizationTests.cs

## Maintenance
- Never modify existing code without reading it first
- All changes must be version-controlled via Git
- Update documentation when implementing new features
- Maintain compatibility with Revision 4.0.0 specifications

## Usage Example
```csharp
var calculator = new ThroughputCalculator();
var result = calculator.Calculate(
    recipe: recipes[0],
    machineSpeed: 0.8
);
// Expected: 480.000 ±0.001 with circuit and splitter optimizations
```

## API Endpoints
N/A - Purely computational library, no HTTP endpoints

## Metrics
- KPI: Calculation accuracy ≥99.9%
- KPI: Throughput precision ±0.001/min
- KPI: CPU usage <0.1% per calculation
- KPI: Circuit capacity utilization ≤100%
- KPI: Splitter efficiency ≥85%

## Related Documentation
- Root README.md: Product vision and requirements
- ../MCP/README.md: MCP integration layer
- ../Graphical/README.md: GUI implementation
