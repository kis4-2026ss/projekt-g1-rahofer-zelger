using System;
using System.Collections.Generic;
using System.Linq;
using Factorio.Core.Models;

namespace Factorio.Core.Services;

/// <summary>
/// Recipe Deserialization Service - handles loading and applying speed multipliers
/// </summary>
public class RecipeDeserializer
{
    /// <summary>
    /// Deserializes recipes from JSON data
    /// </summary>
    public List<Recipe> InitializeRecipes()
    {
        var recipes = new List<Recipe>();
        return recipes;
    }

    /// <summary>
    /// Applies speed multiplier to a recipe
    /// Formula: EffectiveCraftingTime = OriginalCraftingTime / SpeedMultiplier
    /// </summary>
    public double ApplySpeedMultiplier(int OriginalCraftingTime, double SpeedMultiplier)
    {
        return OriginalCraftingTime / SpeedMultiplier;
    }
}

/// <summary>
/// Circuit constraint solver for network validation
/// </summary>
public class CircuitSolver
{
    private readonly List<CircuitDevice> _devices;

    public CircuitSolver(List<CircuitDevice> devices)
    {
        _devices = devices;
    }

    /// <summary>
    /// Solves item targets for circuit networks
    /// </summary>
    public List<ItemTarget> SolveItemTargets()
    {
        var targets = new List<ItemTarget>();
        foreach (var device in _devices)
        {
            foreach (var signal in device.Signals)
            {
                targets.Add(new ItemTarget
                {
                    ItemId = signal.Key,
                    TargetValue = signal.Value,
                    Device = device.Id
                });
            }
        }
        return targets;
    }

    /// <summary>
    /// Validates circuit network constraints
    /// </summary>
    public bool ValidateConstraints()
    {
        return _devices.Count > 0;
    }
}

/// <summary>
/// Production chain analysis service
/// </summary>
public class ProductionChainAnalyzer
{
    private readonly FactorySave _factory;

    public ProductionChainAnalyzer(FactorySave factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Builds production chain graph from factory configuration
    /// </summary>
    public FactorySave BuildChainGraph()
    {
        foreach (var node in _factory.Nodes)
        {
            node.Throughput = CalculateNodeThroughput(node.Machine);
            node.Machine.RecipeCraftingTime = node.Machine.EffectiveCraftingTime;
            node.Machine.RecipeOutputQty = node.Machine.MachineType;
        }
        return _factory;
    }

    /// <summary>
    /// Calculates throughput for a machine node
    /// Formula: T = (RecipeOutputQty / RecipeCraftingTime) * MachineSpeed * 60
    /// </summary>
    public double CalculateNodeThroughput(Machine machine)
    {
        if (machine.RecipeCraftingTime <= 0)
            return 0;

        return (machine.RecipeOutputQty / machine.RecipeCraftingTime) * machine.SpeedMultiplier * 60;
    }

    /// <summary>
    /// Evaluates a production chain from ore to final product
    /// </summary>
    public ProductionResult EvaluateFactoryLine()
    {
        var chainThroughputs = _factory.Nodes
            .Select(node => node.Throughput)
            .ToList();

        var netThroughput = chainThroughputs.Min();

        return new ProductionResult
        {
            ChainThroughputs = chainThroughputs,
            NetChainThroughput = netThroughput,
            BottleneckFactor = netThroughput / Math.Max(netThroughput, 1)
        };
    }
}

/// <summary>
/// Bottleneck detection and analysis service
/// </summary>
public class BottleneckAnalyzer
{
    private readonly FactorySave _factory;

    public BottleneckAnalyzer(FactorySave factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Identifies supply chain bottlenecks
    /// </summary>
    public List<BottleneckReport> FindBottlenecks()
    {
        var bottlenecks = new List<BottleneckReport>();

        foreach (var edge in _factory.Edges)
        {
            var sourceMachine = _factory.Machines[edge.SourceId];
            var targetMachine = _factory.Machines[edge.TargetId];

            // Check upstream capacity vs downstream demand
            var upstreamCapacity = sourceMachine.Throughput;
            var downstreamDemand = targetMachine.Throughput * (edge.MaterialFlow.Values.Max() ?? 0);

            if (upstreamCapacity < downstreamDemand)
            {
                bottlenecks.Add(new BottleneckReport
                {
                    Node = targetMachine.Id,
                    Type = "production_capacity",
                    Deficit = downstreamDemand - upstreamCapacity,
                    CurrentThroughput = upstreamCapacity,
                    Capacity = downstreamDemand,
                    Recommendation = "increase_speed"
                });
            }
        }

        return bottlenecks;
    }

    /// <summary>
    /// Analyzes bottleneck impact
    /// </summary>
    public BottleneckAnalyzeResult AnalyzeBottleneckImpact(BottleneckReport bottleneck)
    {
        return new BottleneckAnalyzeResult
        {
            BottleneckLocation = bottleneck.Node,
            DeficitThroughput = bottleneck.Deficit,
            RecommendedSpeedUp = CalculateRecommendedSpeedUp(bottleneck)
        };
    }

    private int CalculateRecommendedSpeedUp(BottleneckReport bottleneck)
    {
        var speedup = Math.Ceiling(100 + (bottleneck.Deficit * 10) / bottleneck.CurrentThroughput).Cast<int>();
        return Math.Min(speedup, 200);
    }
}

/// <summary>
/// Bottleneck report data transfer object
/// </summary>
public class BottleneckReport
{
    public string Node { get; set; } = "";
    public string Type { get; set; } = "";
    public double Deficit { get; set; }
    public double CurrentThroughput { get; set; }
    public double Capacity { get; set; }
    public string Recommendation { get; set; } = "";
}

/// <summary>
/// Bottleneck analysis result
/// </summary>
public class BottleneckAnalyzeResult
{
    public string BottleneckLocation { get; set; } = "";
    public double DeficitThroughput { get; set; }
    public int RecommendedSpeedUp { get; set; }
}

/// <summary>
/// Production result from factory line evaluation
/// </summary>
public class ProductionResult
{
    public List<double> ChainThroughputs { get; set; } = new();
    public double NetChainThroughput { get; set; }
    public double BottleneckFactor { get; set; }
}
