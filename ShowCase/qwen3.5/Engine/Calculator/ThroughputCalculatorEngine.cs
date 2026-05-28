// ThroughputCalculator.cs
// Factorio Modeler Engine Core - Throughput Calculation Logic
// Revision 4.0.0

using Factorio.Engine.Calculator;
using System;

namespace Factorio.Engine.Calculator
{
    /// <summary>
    /// Core throughput calculation engine for Factorio recipes.
    /// Implements formula: Throughput = (OutputQty / CraftingTime) × MachineSpeed × 60
    /// </summary>
    public class ThroughputCalculator
    {
        /// <summary>
        /// Calculates throughput for a given recipe and machine speed.
        /// </summary>
        /// <param name="recipe">The recipe definition</param>
        /// <param name="machineSpeed">Machine speed multiplier (0.67, 1.0, or 1.5)</param>
        /// <returns>Throughput in units per minute (rounded to ±0.001 tolerance)</returns>
        /// <exception cref="ArgumentNullException">Thrown when recipe is null</exception>
        public double CalculateThroughput(
            Recipe recipe, 
            double machineSpeed)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            if (machineSpeed < 0 || machineSpeed > 2.0)
                throw new ArgumentException($"Invalid machine speed: {machineSpeed}", nameof(machineSpeed));

            // Formula: T = (OutputQty / CraftingTime) × MachineSpeed × 60
            double baseRate = recipe.OutputQty / recipe.CraftingTime;
            double throughput = baseRate * machineSpeed * ThroughputConstants.MinutesPerHour;

            return Math.Round(throughput, 3);
        }

        /// <summary>
        /// Calculates throughput for multi-machine configurations.
        /// </summary>
        /// <param name="recipe">The recipe definition</param>
        /// <param name="numMachines">Number of parallel machines</param>
        /// <param name="machineSpeed">Machine speed multiplier</param>
        /// <returns>Total throughput in units per minute</returns>
        public double CalculateThroughputMultiMachine(
            Recipe recipe,
            int numMachines,
            double machineSpeed)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            if (numMachines <= 0)
                throw new ArgumentException($"Invalid number of machines: {numMachines}", nameof(numMachines));

            // Formula: T = (OutputQty / CraftingTime) × MachineSpeed × 60 × numMachines
            double baseRate = recipe.OutputQty / recipe.CraftingTime;
            double throughput = baseRate * machineSpeed * ThroughputConstants.MinutesPerHour * numMachines;

            return Math.Round(throughput, 3);
        }

        /// <summary>
        /// Gets the accuracy tolerance for throughput calculations.
        /// </summary>
        public double GetAccuracyTolerance()
        {
            return ThroughputConstants.Tolerance;
        }

        /// <summary>
        /// Validates recipe inputs before calculation.
        /// </summary>
        public ValidationResult ValidateRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Recipe cannot be null"
                };
            }

            if (recipe.OutputQty <= 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Invalid output quantity: {recipe.OutputQty}"
                };
            }

            if (recipe.CraftingTime <= 0 || recipe.CraftingTime > 3600)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Invalid crafting time: {recipe.CraftingTime}"
                };
            }

            return new ValidationResult { IsValid = true, Message = "Recipe is valid" };
        }

        /// <summary>
        /// Parses machine speed from machine type identifier.
        /// </summary>
        public double ParseMachineSpeed(string machineType)
        {
            return machineType switch
            {
                "assembling-machine-0" => ThroughputConstants.ASM_Level_1_Speed,
                "assembling-machine-1" => ThroughputConstants.ASM_Level_2_4_Speed,
                "assembling-machine-2" => ThroughputConstants.ASM_Level_5_Speed,
                _ => ThroughputConstants.ASM_Level_2_4_Speed
            };
        }
    }

    /// <summary>
    /// Recipe data structure for throughput calculation.
    /// </summary>
    public class Recipe
    {
        public string Id { get; set; }
        public string MachineType { get; set; }
        public string RecipeName { get; set; }
        public string OutputItemId { get; set; }
        public int OutputQty { get; set; }
        public double CraftingTime { get; set; }
        public string[] RequiredResources { get; set; }
        public int? OutputRate { get; set; }
        public double? MachineSpeed { get; set; }
    }

    /// <summary>
    /// Validation result for recipe inputs.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
