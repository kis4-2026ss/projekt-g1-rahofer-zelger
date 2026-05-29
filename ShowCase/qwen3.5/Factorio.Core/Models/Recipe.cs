using System.Collections.Generic;

namespace Factorio.Core.Models;

/// <summary>
/// Represents a Factorio crafting recipe with output quantity and crafting time.
/// </summary>
public sealed class Recipe
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public string OutputItemId { get; set; } = string.Empty;
    public int OutputQty { get; set; }
    public decimal CraftingTime { get; set; }
    public string? RecipeVariant { get; set; }
    public List<ResourceRequirement> RequiredResources { get; set; } = new();

    /// <summary>
    /// Gets/sets the machine speed multiplier (1.0 = 100%)
    /// </summary>
    public double MachineSpeedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets effective crafting time based on speed multiplier
    /// </summary>
    public decimal EffectiveCraftingTime => CraftingTime / MachineSpeedMultiplier;

    public sealed class ResourceRequirement
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int Minutely { get; set; }
    }
}

/// <summary>
/// Represents a machine entity with crafting speed multiplier and icon.
/// </summary>
public sealed class Machine
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public double CraftingSpeed { get; set; } = 100.0;
    public bool IsPowered { get; set; }
    public int MaxItemCount { get; set; }
    public MachineCategory Category { get; set; }
    public string Emoji { get; set; } = string.Empty;

    public enum MachineCategory
    {
        Industrial,
        Crafting,
        Conveyor,
        Storage,
        Circuit,
        Module,
        Tank
    }
}

/// <summary>
/// Represents a production node in the graph.
/// </summary>
public sealed class Node
{
    public string NodeType { get; set; } = string.Empty;
    public string GraphNodeId { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public double MachineSpeed { get; set; }
    public double Throughput { get; set; }
    public string RecipeId { get; set; } = string.Empty;
    public string RecipeVariant { get; set; } = string.Empty;
}

/// <summary>
/// Represents a production chain graph with nodes and edges.
/// </summary>
public sealed class Graph
{
    public List<Node> Nodes { get; set; } = new();
    public List<Edge> Edges { get; set; } = new();

    /// <summary>
    /// Represents an edge in the production graph.
    /// </summary>
    public sealed class Edge
    {
        public string SourceNodeId { get; set; } = string.Empty;
        public string TargetNodeId { get; set; } = string.Empty;
        public double FlowRate { get; set; }
        public int RequiredQty { get; set; }
    }
}

/// <summary>
/// Graph traversal result for production flow analysis.
/// </summary>
public sealed class TraversalResult
{
    public List<string> Path { get; set; } = new();
    public double TotalChainThroughput { get; set; }
    public List<string> IntermediateNodes { get; set; } = new();
}

/// <summary>
/// Represents analysis results for a production chain.
/// </summary>
public sealed class AnalysisResult
{
    public string ItemName { get; set; } = string.Empty;
    public double NetRate { get; set; }
    public int UpstreamCount { get; set; }
    public int DownstreamCount { get; set; }
    public List<AnalysisItem> Upstream { get; set; } = new();
    public List<AnalysisItem> Downstream { get; set; } = new();
}

/// <summary>
/// Individual analysis item.
/// </summary>
public sealed class AnalysisItem
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double Rate { get; set; }
    public double Queued { get; set; }
}

/// <summary>
/// Bottleneck analysis result.
/// </summary>
public sealed class Bottleneck
{
    public string Node { get; set; } = string.Empty;
    public string? Type { get; set; }
    public double Deficit { get; set; }
    public double Capacity { get; set; }
    public double CurrentThroughput { get; set; }
    public string? Recommendation { get; set; }
}

/// <summary>
/// Factory save entity containing all production data.
/// </summary>
public sealed class FactorySave
{
    public string Id { get; set; } = string.Empty;
    public int SaveId { get; set; }
    public string Version { get; set; } = "1.0";
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<Machine> Machines { get; set; } = new List<Machine>();
    public ICollection<Node> Nodes { get; set; } = new List<Node>();
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
    public string? Path { get; set; }
    public double TotalThroughput { get; set; }
    public Bottleneck? Bottleneck { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
