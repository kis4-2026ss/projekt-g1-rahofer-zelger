using System;
using System.Collections.Generic;
using System.Linq;
using Factorio.Core.Models;
using Factorio.Core.Services;

namespace Factorio.Core.Services;

/// <summary>
/// Distribution network optimizer using modified Bellman-Ford
/// </summary>
public class DistributionOptimizer
{
    private readonly Dictionary<int, MachineEntity> _machines;
    private readonly List<BeltEntity> _belts;

    public DistributionOptimizer(List<MachineEntity> machines, List<BeltEntity> belts)
    {
        _machines = machines.ToDictionary(m => m.X);
        _belts = belts;
    }

    /// <summary>
    /// Find optimal belt routing from sources to sinks
    /// Uses modified Bellman-Ford for weighted graphs
    /// </summary>
    public OptimizationResult Optimize(List<MachineEntity> sources, List<MachineEntity> sinks)
    {
        var distance = new Dictionary<int, double>();
        var previous = new Dictionary<int, int>();
        var path = new Dictionary<int, List<int>>();

        // Initialize
        foreach (var machine in sources)
        {
            distance[machine.X] = 0;
        }

        // Bellman-Ford iterations
        for (int i = 0; i < _machines.Count; i++)
        {
            bool changed = false;
            foreach (var machine in _machines.Keys)
            {
                foreach (var belt in _belts)
                {
                    if (belt.X == machine.X)
                    {
                        var altDistance = distance[machine.X] + 1; // Simplified weight
                        if (altDistance < (distance.ContainsKey(belt.X) ? distance[belt.X] : int.MaxValue))
                        {
                            distance[belt.X] = altDistance;
                            previous[belt.X] = machine.X;
                            changed = true;
                        }
                    }
                }
            }

            if (!changed && distance.Values.All(d => d <= 1))
                break;
        }

        // Reconstruct optimal paths
        foreach (var sink in sinks)
        {
            path[sink.X] = new List<int>();
            var current = sink.X;
            while (current != 0)
            {
                path[sink.X].Add(current);
                current = previous.GetValueOrDefault(current, 0);
            }
        }

        return new OptimizationResult
        {
            ObjectiveValue = distance.Values.Sum(),
            Solution = path
        };
    }

    /// <summary>
    /// Calculate buffer requirements
    /// </summary>
    public int CalculateBufferRequirements(List<MachineEntity> machines)
    {
        // Simplified buffer calculation
        return Math.Max(10, machines.Count * 5);
    }
}
