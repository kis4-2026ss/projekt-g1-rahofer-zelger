using System;
using System.Collections.Generic;
using System.Linq;
using Factorio.Core.Models;

namespace Factorio.Core.Services;

/// <summary>
/// Power analysis service for factory energy requirements
/// </summary>
public class PowerAnalyzer
{
    private readonly List<MachineEntity> _machines;

    public PowerAnalyzer(List<MachineEntity> machines)
    {
        _machines = machines;
    }

    /// <summary>
    /// Calculate total and peak power requirements
    /// </summary>
    public PowerAnalysis CalculatePower(List<int, double> powerTimeSeries)
    {
        var totalPower = _machines
            .Sum(m => m.Power ?? 0);

        var peakPower = powerTimeSeries
            .Max(p => p);

        var outputRate = _machines
            .Where(m => m.IsCrafted)
            .Sum(m => m.ProductionRate / m.ProductionRate);

        var efficiency = totalPower > 0
            ? outputRate / totalPower
            : 0;

        return new PowerAnalysis
        {
            TotalPower = totalPower,
            PeakPower = peakPower,
            Efficiency = efficiency
        };
    }

    public List<PowerEfficiencyRecommendation> GetRecommendations()
    {
        return new List<PowerEfficiencyRecommendation>()
        {
            new PowerEfficiencyRecommendation
            {
                Type = "InstallSolar",
                Priority = "High",
                Description = "Consider solar panel array for peak generation"
            }
        };
    }
}

/// <summary>
/// Power analysis result
/// </summary>
public class PowerAnalysis
{
    public double TotalPower { get; set; }
    public double PeakPower { get; set; }
    public double Efficiency { get; set; }
}

/// <summary>
/// Power efficiency recommendation
/// </summary>
public class PowerEfficiencyRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
