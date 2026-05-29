using System.Text.Json.Serialization;

namespace Factorio.Core.Models;

/// <summary>
/// Represents a recipe with output quantity, crafting time, and required ingredients
/// </summary>
public class Recipe
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("recipe")]
    public RecipeDefinition RecipeDefinition { get; set; } = null!;

    [JsonIgnore]
    public int RecipeOutputQty => RecipeDefinition.Yield ?? 0;

    [JsonIgnore]
    public double RecipeCraftingTime => RecipeDefinition.Time ?? 0;

    [JsonPropertyName("recipe")]
    public class RecipeDefinition
    {
        [JsonPropertyName("time")]
        public double? Time { get; set; }

        [JsonPropertyName("yield")]
        public int? Yield { get; set; }

        [JsonPropertyName("ingredients")]
        public List<Ingredient>[]? Ingredients => new List<Ingredient>[]
        {
            new List<Ingredient>()
        };

        [JsonPropertyName("ingredients")]
        public List<Ingredient> Items { get; set; } = new();

        [JsonPropertyName("ingredients")]
        public int IngredientCount => Ingredients?.Length ?? 0;
    }

    [JsonPropertyName("recipe")]
    public class Ingredient
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}

/// <summary>
/// Represents a machine entity with speed and status
/// </summary>
public class Machine
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string RecipeType { get; set; } = "";
    public double SpeedMultiplier { get; set; } = 1.0;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets calculated throughput in items/minute
    /// Formula: T = (RecipeOutputQty / RecipeCraftingTime) * MachineSpeed * 60
    /// </summary>
    public double Throughput { get; set; }

    /// <summary>
    /// Gets required crafting time based on speed multiplier
    /// </summary>
    public double EffectiveCraftingTime => RecipeCraftingTime / SpeedMultiplier;

    [JsonIgnore]
    public double RecipeCraftingTime => 0;

    [JsonIgnore]
    public int RecipeOutputQty => 0;
}

/// <summary>
/// Represents a production chain node
/// </summary>
public class Node
{
    public string Id { get; set; } = "";
    public Machine Machine { get; set; } = null!;
    public int NodeIndex { get; set; }

    /// <summary>
    /// Gets calculated node throughput
    /// </summary>
    public double Throughput => Machine.Throughput;

    [JsonIgnore]
    public string RecipeName => Machine.Type;
}

/// <summary>
/// Represents a material flow connection between machines
/// </summary>
public class Edge
{
    public string SourceId { get; set; } = "";
    public string TargetId { get; set; } = "";
    public Dictionary<string, double> MaterialFlow { get; set; } = new();
    public string ConnectionType { get; set; } = "direct";
    public bool IsCircuitConnected { get; set; } = false;
}

/// <summary>
/// Represents complete factory save state
/// </summary>
public class FactorySave
{
    public string Name { get; set; } = "";
    public List<Node> Nodes { get; set; } = new();
    public List<Edge> Edges { get; set; } = new();
    public Dictionary<string, Machine> Machines { get; set; } = new();
    public List<Belt> Belts { get; set; } = new();
    public List<CircuitDevice> Circuits { get; set; } = new();

    /// <summary>
    /// Adds a new node to the production chain
    /// </summary>
    public Node AddNode(Node node)
    {
        Nodes.Add(node);
        Machines[node.Id] = node.Machine;
        return node;
    }

    /// <summary>
    /// Adds a material flow connection
    /// </summary>
    public Edge AddEdge(Edge edge)
    {
        Edges.Add(edge);
        return edge;
    }
}

/// <summary>
/// Represents a conveyor belt entity
/// </summary>
public class Belt
{
    public string Id { get; set; } = "";
    public bool IsSaturated { get; set; }
    public int Throughput { get; set; } = 96;
    public List<string> ItemsOnBelt { get; set; } = new();
}

/// <summary>
/// Represents a circuit network device
/// </summary>
public class CircuitDevice
{
    public string Id { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public double OutputValue { get; set; }
    public Dictionary<string, double> Signals { get; set; } = new();
}
