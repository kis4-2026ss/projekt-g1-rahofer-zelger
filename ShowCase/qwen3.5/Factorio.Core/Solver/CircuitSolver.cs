using System;
using System.Collections.Generic;
using System.Linq;

namespace Factorio.Core.Solver
{
    /// <summary>
    /// Solver for circuit networks.
    /// </summary>
    public class CircuitSolver
    {
        private readonly List<CircuitDevice> _devices;

        /// <summary>
        /// Initializes a new instance with circuit devices.
        /// </summary>
        /// <param name="devices">List of circuit devices.</param>
        public CircuitSolver(List<CircuitDevice> devices)
        {
            _devices = devices;
        }

        /// <summary>
        /// Solves item targets for circuit devices.
        /// </summary>
        public CircuitSolution SolveItemTargets()
        {
            var solution = new CircuitSolution();

            // Sort devices by ID for deterministic processing
            var sortedDevices = _devices.OrderByDescending(d => d.Id).ToList();

            foreach (var device in sortedDevices)
            {
                solution.DevicesById.TryGetValue(device.Id, out var existing);
                if (!existing)
                {
                    device.TargetItems = new List<string>();
                    solution.DevicesById[device.Id] = device;
                }
                else
                {
                    existing.TargetItems = device.TargetItems;
                }
            }

            return solution;
        }

        /// <summary>
        /// Validates circuit constraints.
        /// </summary>
        public List<CircuitContradiction> ValidateConstraints()
        {
            var contradictions = new List<CircuitContradiction>();

            foreach (var wire in _devices.Where(d => d.Type == "circuit-network-wire"))
            {
                if (wire.Saturation > 0)
                {
                    contradictions.Add(new CircuitContradiction
                    {
                        Description = $"Wire {wire.Id} has saturation {wire.Saturation}%",
                        Device = wire.Id,
                        DeviceType = wire.Type
                    });
                }
            }

            return contradictions;
        }
    }

    /// <summary>
    /// Represents a circuit device.
    /// </summary>
    public class CircuitDevice
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Saturation { get; set; } = 0;
        public List<string>? TargetItems { get; set; }
        public List<string>? ConnectedCircuits { get; set; }
    }

    /// <summary>
    /// Solution from circuit solving.
    /// </summary>
    public class CircuitSolution
    {
        public List<string> Wires { get; set; } = new List<string>();
        public Dictionary<string, CircuitDevice> DevicesById { get; set; } = new Dictionary<string, CircuitDevice>();
    }

    /// <summary>
    /// Circuit contradiction.
    /// </summary>
    public class CircuitContradiction
    {
        public string Description { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
    }
}
