using System.Collections.Generic;

namespace FactorioModeler.Engine.Models;

/// ✏️ Factory recipe definition including output specs and resource requirements
public class Recipe
{
    public string Id { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public string OutputItemId { get; set; } = string.Empty;
    public int OutputQty { get; set; }
    public int CraftingTime { get; set; }
    public List<RequiredResource> RequiredResources { get; set; } = new();
    public double OutputRate { get; set; }
}

/// 📦 Resource requirement per recipe
public class RequiredResource
{
    public string ItemId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int Minutely { get; set; }
}

/// 🏗️ Factory machine configuration
public class Machine
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public double MinSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public string Category { get; set; } = string.Empty;
    public int MaxOutputs { get; set; }
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public double BaseSpeed { get; set; }
}

/// 🔄 Production graph node representation
public class GraphNode
{
    public string Id { get; set; } = string.Empty;
    public string ItemTypeId { get; set; } = string.Empty;
    public string MachineTypeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double Throughput { get; set; }
}
