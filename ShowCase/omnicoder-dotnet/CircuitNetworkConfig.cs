using System.Text.Json;
using System.Text.Json.Serialization;
using FactorioModeler.Engine.Models;

namespace FactorioModeler.Engine.Services;

/// 🔌 Circuit network configuration service
/// Manages circuit network setups for production optimization
public class CircuitNetworkConfig
{
    /// 📋 Calculate circuit network requirements for a recipe
    /// <param name="recipe">Recipe requiring circuit network configuration</param>
    /// <returns>Circuit network configuration with power/signal requirements</returns>
    public static CircuitNetworkConfiguration CalculateNetworkConfig(Recipe recipe)
    {
        var resources = new List<CircuitResource>();

        foreach (var resource in recipe.RequiredResources)
        {
            resources.Add(new CircuitResource()
            {
                ItemId = resource.ItemId,
                Signal = resource.Minutely,
                Color = GetResourceColor(resource.ItemId)
            });
        }

        return new CircuitNetworkConfiguration()
        {
            RecipeId = recipe.Id,
            RecipeName = recipe.RecipeName,
            Resources = resources,
            PowerSignal = resources.Sum(r => r.Signal * 0.5), // Estimate power
            MaxVoltage = 50000, // Standard circuit network max
            NetworkType = resources.Any(r => r.Minutely > 100) 
                ? CircuitNetworkType.Advanced 
                : CircuitNetworkType.Standard
        };
    }

    /// 🎨 Get color for resource signal
    private static string GetResourceColor(string itemId)
    {
        return itemId.ToLower() switch
        {
            var x when x.Contains("copper") => "#4a5d23",
            var x when x.Contains("iron") => "#7a7a7a",
            var x when x.Contains("steel") => "#4d4d4d",
            var x when x.Contains("gold") => "#ffd700",
            var x when x.Contains("advanced") => "#c0c0c0",
            "red",
            _ => "#ffffff"
        };
    }

    /// 🔌 Validate circuit network configuration
    public static bool ValidateConfiguration(CircuitNetworkConfiguration config)
    {
        if (config == null) return false;

        return config.NetworkType != CircuitNetworkType.None
            && config.Resources != null
            && config.PowerSignal < config.MaxVoltage;
    }

    /// 📊 Calculate circuit network efficiency
    public static double CalculateNetworkEfficiency(CircuitNetworkConfiguration config)
    {
        if (config == null) return 0;

        var actualVoltage = Math.Min(config.PowerSignal, config.MaxVoltage);
        var efficiency = (actualVoltage / (double)config.MaxVoltage) * 100;

        return efficiency;
    }

    /// 🎯 Advanced circuit network calculations
    public static AdvancedCircuitConfiguration CalculateAdvancedCircuit()
    {
        return new AdvancedCircuitConfiguration()
        {
            Type = "Advanced Circuit",
            RequiredInputs = new List<string> { "copper-plate", "iron-plate", "steel-plate" },
            SignalStrength = 10,
            MaxSignal = 2147483647,
            PowerRequirement = 80
        };
    }

    /// 🎯 Express splitter circuit configuration
    public static ExpressSplitterConfiguration CalculateExpressSplitter()
    {
        return new ExpressSplitterConfiguration()
        {
            Type = "Express Splitter",
            RequiredSignals = new List<string> { "copper-plate", "iron-plate", "advanced-circuit" },
            SignalStrength = 5,
            MaxSignal = 100,
            PowerRequirement = 30
        };
    }
}

/// 🎯 Circuit network configuration result
public class CircuitNetworkConfiguration
{
    public string RecipeId { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public List<CircuitResource> Resources { get; set; } = new();
    public double PowerSignal { get; set; }
    public int MaxVoltage { get; set; } = 50000;
    public CircuitNetworkType NetworkType { get; set; }
}

/// 📦 Circuit resource in network
public class CircuitResource
{
    public string ItemId { get; set; } = string.Empty;
    public int Signal { get; set; }
    public string Color { get; set; } = "#ffffff";
    public int Minutely { get; set; }
}

/// 🌐 Circuit network types
public enum CircuitNetworkType
{
    Standard,
    Advanced,
    None
}

/// 🎯 Advanced circuit configuration
public class AdvancedCircuitConfiguration
{
    public string Type { get; set; } = "Advanced Circuit";
    public List<string> RequiredInputs { get; set; } = new();
    public int SignalStrength { get; set; } = 10;
    public int MaxSignal { get; set; } = 2147483647;
    public int PowerRequirement { get; set; } = 80;
}

/// 🎯 Express splitter configuration
public class ExpressSplitterConfiguration
{
    public string Type { get; set; } = "Express Splitter";
    public List<string> RequiredSignals { get; set; } = new();
    public int SignalStrength { get; set; } = 5;
    public int MaxSignal { get; set; } = 100;
    public int PowerRequirement { get; set; } = 30;
}
