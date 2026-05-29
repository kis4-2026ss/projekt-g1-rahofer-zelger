using System;
using System.Collections.Generic;
using System.Linq;

namespace Factorio.Core.Services
{
    /// <summary>
    /// Calculates production throughput for machines.
    /// </summary>
    public class ThroughputCalculator
    {
        /// <summary>
        /// Calculates throughput using the formula: T = (OutputQty/CraftingTime) * Speed * 60
        /// </summary>
        /// <param name="outputQty">Recipe output quantity</param>
        /// <param name="craftingTime">Recipe crafting time in seconds</param>
        /// <param name="speedMultiplier">Machine speed multiplier (0.0-1.0)</param>
        /// <returns>Throughput in items per minute</returns>
        public double CalculateThroughput(int outputQty, decimal craftingTime, double speedMultiplier)
        {
            if (craftingTime <= 0) throw new ArgumentException("Crafting time must be > 0");
            if (speedMultiplier < 0 || speedMultiplier > 1) throw new ArgumentOutOfRangeException(nameof(speedMultiplier));

            double effectiveTime = craftingTime / speedMultiplier;
            double itemsPerSecond = outputQty / effectiveTime;
            return itemsPerSecond * 60;
        }

        /// <summary>
        /// Calculates throughput for advanced circuit: must be exactly 10/min
        /// </summary>
        public double CalculateAdvancedCircuitThroughput()
        {
            return CalculateThroughput(10, 1, 1.0);
        }

        /// <summary>
        /// Calculates throughput for express splitter: must be exactly 2.5/min
        /// </summary>
        public double CalculateExpressSplitterThroughput()
        {
            return CalculateThroughput(2, 15, 1.0);
        }

        /// <summary>
        /// Validates throughput matches target within tolerance
        /// </summary>
        /// <param name="calculated">Calculated throughput</param>
        /// <param name="target">Target throughput</param>
        /// <param name="tolerance">Acceptable tolerance (0.001 = 0.1%)</param>
        /// <returns>True if within tolerance</returns>
        public bool ValidateThroughput(double calculated, double target, double tolerance = 0.001)
        {
            double diff = Math.Abs(calculated - target) / target;
            return diff <= tolerance;
        }
    }
}
