using System.Text.Json.Serialization;

namespace Factorio.Core.Models;

/// <summary>
/// Item target for circuit network
/// </summary>
public class ItemTarget
{
    [JsonPropertyName("item")]
    public string ItemId { get; set; } = "";

    [JsonPropertyName("target")]
    public double TargetValue { get; set; }

    [JsonPropertyName("device")]
    public string Device { get; set; } = "";
}
