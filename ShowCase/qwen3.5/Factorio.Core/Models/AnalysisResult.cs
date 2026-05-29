using System;
using System.Collections.Generic;

namespace Factorio.Core.Models;

/// <summary>
/// Analysis result for production chains
/// </summary>
public class AnalysisResult
{
    public string ItemName { get; set; } = string.Empty;
    public int UpstreamCount { get; set; }
    public int DownstreamCount { get; set; }
    public double NetRate { get; set; }
    public List<CircuitAnalysis>? CircuitAnalysis { get; set; }
    public List<BeltAnalysis>? BeltAnalysis { get; set; }
}

/// <summary>
/// Circuit analysis item
/// </summary>
public class CircuitAnalysis
{
    public string ItemName { get; set; } = string.Empty;
    public int WireCount { get; set; }
    public bool Validated { get; set; }
}

/// <summary>
/// Belt analysis
/// </summary>
public class BeltAnalysis
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Actual { get; set; }
    public double SaturationRatio { get; set; }
}

/// <summary>
/// Optimization result for belt paths
/// </summary>
public class OptimizationResult
{
    public double ObjectiveValue { get; set; }
    public Dictionary<int, int> Solution { get; set; } = new();
}
