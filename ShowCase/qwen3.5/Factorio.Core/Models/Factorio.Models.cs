using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Factorio.Core
{
    /// <summary>
    /// Represents a recipe configuration for production machines.
    /// </summary>
    public class Recipe
    {
        public string Id { get; set; } = string.Empty;
        public string MachineType { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public int OutputQty { get; set; }
        public decimal CraftingTime { get; set; }
        public ICollection<ResourceRequirement> RequiredResources { get; set; } = new List<ResourceRequirement>();
        public double OutputRate { get; set; }
        public int EffectiveCraftingTime { 
            get 
            { 
                if (MachineSpeed == 0) return (int)(CraftingTime / 1.0);
                return (int)(CraftingTime / MachineSpeed);
            } 
        }
        
        public double MachineSpeed { get; set; } = 1.0;
    }

    /// <summary>
    /// Resource required for crafting.
    /// </summary>
    public class ResourceRequirement
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int Minutely { get; set; }
    }

    /// <summary>
    /// Machine entity definition.
    /// </summary>
    public class Machine
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🏭";
        public double MinSpeed { get; set; } = 0.0;
        public double MaxSpeed { get; set; } = 1.0;
        public string Category { get; set; } = "industrial";
        public int MaxOutputs { get; set; } = 10;
        public string CategoryColor { get; set; } = "red";
        public string CategoryName { get; set; } = "Machine";
    }

    /// <summary>
    /// Node representing a machine instance.
    /// </summary>
    public class Node
    {
        public string Id { get; set; } = "mach-001";
        public string MachineType { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string RecipeId { get; set; } = string.Empty;
        public double Throughput { get; set; }
        public double SpeedMultiplier { get; set; } = 1.0;
        public Node? UpstreamNode { get; set; }
        public ICollection<Edge> OutgoingEdges { get; set; } = new List<Edge>();
        public string MachineName { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🏭";
        public string Status { get; set; } = "active";
    }

    /// <summary>
    /// Edge representing material flow.
    /// </summary>
    public class Edge
    {
        public string SourceNode { get; set; } = string.Empty;
        public string TargetNode { get; set; } = string.Empty;
        public string MaterialId { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public double FlowRate { get; set; } = 0.0;
        public bool HasCircuit { get; set; } = false;
    }

    /// <summary>
    /// Graph representing the production chain.
    /// </summary>
    public class ProductionChain
    {
        public ICollection<Node> Nodes { get; set; } = new List<Node>();
        public ICollection<Edge> Edges { get; set; } = new List<Edge>();
        public double TotalThroughput { get; set; }
        public Bottleneck? Bottleneck { get; set; }
    }

    /// <summary>
    /// Bottleneck information.
    /// </summary>
    public class Bottleneck
    {
        public string NodeId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double DeficitThroughput { get; set; }
        public double CurrentThroughput { get; set; }
        public double Capacity { get; set; }
        public string? Recommendation { get; set; }
    }
}
