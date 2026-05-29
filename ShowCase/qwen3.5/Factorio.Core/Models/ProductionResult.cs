using System;

namespace Factorio.Core.Models;

/// <summary>
/// Production result DTO
/// </summary>
public class ProductionResult
{
    [JsonPropertyName("chainThroughputs")]
    public double[] ChainThroughputs { get; set; } = Array.Empty<double>();

    [JsonPropertyName("netChainThroughput")]
    public double NetChainThroughput { get; set; }

    [JsonPropertyName("bottleneckFactor")]
    public double BottleneckFactor { get; set; }

    [JsonPropertyName("achieved")]
    public bool Achieved { get; set; }
}
