namespace Factorio.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a machine entity with crafting capabilities.
/// </summary>
public class Machine
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public double CraftingSpeedMultiplier { get; set; } // 1.0 = 100%, 2.0 = 200%
    public int MaxItemCount { get; set; }
    public bool IsPowered { get; set; }
    public List<Circuit> Circuits { get; set; } = new();
    public double PowerConsumption { get; set; }

    public class Circuit
    {
        public string? TargetItem { get; set; }
        public double OutputPerMinute { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Gets effective crafting speed multiplier
    /// </summary>
    public double GetEffectiveCraftingSpeed()
    {
        if (!IsPowered)
            return 0;

        // Start with base speed multiplier
        double effectiveSpeed = CraftingSpeedMultiplier;

        // Apply productivity from modules (simplified)
        foreach (var circuit in Circuits)
        {
            if (circuit.TargetItem != null || circuit.OutputPerMinute > 0)
            {
                effectiveSpeed += circuit.OutputPerMinute;
            }
        }

        return effectiveSpeed;
    }

    /// <summary>
    /// Validates machine health status
    /// </summary>
    public MachineHealthStatus ValidateHealth()
    {
        if (!IsPowered)
            return MachineHealthStatus.Unpowered;

        if (MaxItemCount <= 0)
            return MachineHealthStatus.Error;

        return MachineHealthStatus.Healthy;
    }

    public enum MachineHealthStatus
    {
        Healthy,
        Unpowered,
        Error
    }
}
