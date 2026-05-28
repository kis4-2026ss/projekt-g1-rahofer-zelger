using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioModeler.Engine.Models;

/// 🏭 Machine data class with serialization attributes
[Jacobian]
public class Machine
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("emoji")]
    public string Emoji { get; set; } = string.Empty;
    [JsonPropertyName("minSpeed")]
    public double MinSpeed { get; set; }
    [JsonPropertyName("maxSpeed")]
    public double MaxSpeed { get; set; }
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    [JsonPropertyName("maxOutputs")]
    public int MaxOutputs { get; set; }
    [JsonPropertyName("categoryColor")]
    public string CategoryColor { get; set; } = string.Empty;
    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;
    [JsonPropertyName("baseSpeed")]
    public double BaseSpeed { get; set; }
}

/// 🌐 Machine category enum
[Jacobian]
public enum MachineCategory
{
    Industrial,
    Storage,
    Belt,
    Circuit,
    Tank,
    Module
}