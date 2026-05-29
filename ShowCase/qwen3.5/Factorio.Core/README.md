# Factorio.Core - Backend Engine

## Overview
Core analytical engine for Factorio factory modeling. Provides production chain analysis, circuit solving, belt capacity modeling, and power analysis.

## Architecture

```
Factorio.Core/
├── Models/                   # Domain entities
│   ├── Entities.cs          # FactorySave, Machine, Belt, Circuit models
│   └── AnalysisResult.cs    # Production chain results
├── Services/                 # Analysis services
│   ├── SaveFileHandler.cs   # Load/Save operations
│   ├── ProductionChainAnalyzer.cs
│   ├── CircuitSolver.cs     # Circuit constraint solver
│   ├── BeltAnalyzer.cs      # Throughput/saturation
│   ├── PowerAnalyzer.cs     # Energy requirements
│   └── DistributionOptimizer.cs
├── Solver/                   # Specialized algorithms
│   └── CircuitSolver.cs     # Network analysis
├── Factories/               # Dependency injection
│   └── SaveFactoryAttribute.cs
└── Features/                # Gherkin feature files
    └── ProductionChain.feature
```

## Module Dependency Graph

```
┌─────────────────────────────────────────────┐
│           Factorio.Core Engine              │
├─────────────────────────────────────────────┤
│ Models → Services → Solver → Features       │
│ (Entities)  (Analysis)  (Network)   (Docs)   │
└─────────────────────────────────────────────┘
```

## Gherkin Feature Stories

See `Features/ProductionChain.feature` for detailed acceptance criteria:

```gherkin
Feature: Production Chain Analysis
  As a factory designer
  I want to analyze production chains
  So I can identify bottlenecks and optimize layouts
```

## Compilation Instructions

```bash
dotnet restore
dotnet build --configuration Release
dotnet build --no-incremental
```

## Test Instructions

```bash
dotnet test --verbosity normal
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

## Configuration

- **Target Framework**: .NET 8.0
- **Package**: Newtonsoft.Json 13.0.3
- **Nullable**: Enabled
- **Implicit Usings**: Enabled

## Environment Requirements

- .NET 8.0+ Runtime
- 1GB RAM minimum
- 50MB build space

## Usage Examples

```csharp
// Load factory
var factory = services.SaveFileHandler.Load("save.dat");

// Analyze production chain

// Solve circuits
var solver = new CircuitSolver(devices);
var targets = solver.SolveItemTargets();

// Check belt saturation
var beltAnalysis = analyzer.CalculateBeltAnalysis(belts);
```

## Unit Tests

- `CircuitSolverTests.cs` - Circuit solver validation
- `BeltAnalyzerTests.cs` - Throughput calculations
- `ProductionChainTests.cs` - Recipe chain analysis
- `PowerAnalyzerTests.cs` - Power requirements
- `SaveFileIntegrationTests.cs` - Save file loading
- `FullPipelineTests.cs` - End-to-end tests

## API Usage

```csharp
// Load save
var factory = services.SaveFileHandler.Load("save.dat");

// Analyze production chain
var result = analyzer.AnalyzeChain("iron-plate", factory);

// Solve circuits
var solver = new CircuitSolver(devices);
var targets = solver.SolveItemTargets();

// Check belt saturation
var beltAnalysis = analyzer.CalculateBeltAnalysis(belts);
```
