using System;
using System.Collections.Generic;
using System.Linq;
using FactorioModeler.Engine.Models;
using FactorioModeler.Engine.Services;

namespace FactorioModeler.Engine.Services;

/// 🧮 Throughput calculation service implementing T = (output_qty / crafting_time) × speed × 60
public class ThroughputCalculator
{
    /// 📐 Calculate throughput formula per specification
    public static double CalculateThroughput(Recipe recipe, string machineId)
    {
        var speed = RecipeLoader.GetMachineCraftingSpeed(machineId);
        return (recipe.OutputQty / (double)recipe.CraftingTime) * speed * 60;
    }

    public static double CalculateThroughput(Recipe recipe, MachineConfig config)
    {
        return (recipe.OutputQty / (double)recipe.CraftingTime) * config.CraftSpeed * 60;
    }

    public static (List<NodeMetrics> nodes, string? bottleneck) AnalyzeLine(List<Recipe> line, Dictionary<string, MachineConfig> configs)
    {
        var nodeMetrics = line.Select(r => new NodeMetrics
        {
            NodeId = r.Id,
            Throughput = CalculateThroughput(r, configs[r.MachineType]!),
            Constraint = null
        }).ToList();

        var bottlenecks = nodeMetrics
            .OrderBy(n => n.Throughput)
            .FirstOrDefault();

        return (nodeMetrics, bottlenecks?.NodeId);
    }

    /// 🔍 Calculate line total throughput at bottleneck
    public static LineMetrics CalculateLineThroughput(List<Recipe> line, Dictionary<string, MachineConfig> configs)
    {
        var (nodeMetrics, bottlenecks) = AnalyzeLine(line, configs);

        return new LineMetrics
        {
            PipelineName = line.First().RecipeName,
            TotalThroughput = nodeMetrics.Min(n => n.Throughput),
            NodeMetrics = nodeMetrics,
            Bottleneck = bottlenecks
        };
    }
}