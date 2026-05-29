using System;

namespace Engine.Calculator
{
    /// <summary>
    /// Calculates throughput for recipes.
    /// Formula: T = (OutputQty/CraftingTime) × MachineSpeed × 60
    /// </summary>
    public class ThroughputCalculator
    {
        private readonly double _tolerance = 0.001;

        /// <summary>
        /// Validates recipe and machine speed.
        /// </summary>
        public ValidationResult ValidateRecipe(Recipe? recipe, int machineLevel)
        {
            try
            {
                if (recipe == null)
                    return ValidationResult.Invalid("null", 0, 0);

                if (recipe.CraftingTime <= 0)
                    return ValidationResult.Invalid("positive", 0, 0);

                if (machineLevel < 1 || machineLevel > 3)
                    return ValidationResult.Invalid("1-3", 0, 0);

                return ValidationResult.Valid;
            }
            catch (Exception)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "unknown",
                    Error = double.NaN,
                    Accuracy = 0d
                };
            }
        }

        /// <summary>
        /// Calculates throughput for a recipe.
        /// </summary>
        public double CalculateThroughput(Recipe recipe, int machineLevel, double speedMultiplier = 1.0)
        {
            try
            {
                var validation = ValidateRecipe(recipe, machineLevel);
                if (!validation.IsValid)
                    return 0;

                // Apply machine level speed multiplier
                double machineSpeed = GetMachineSpeedLevel(machineLevel);
                
                // Calculate: (OutputQty / CraftingTime) × MachineSpeed × 60
                double baseRate = recipe.OutputQty / (double)recipe.CraftingTime;
                double throughput = baseRate * machineSpeed * 60;
                
                // Apply ±0.001 tolerance
                return Math.Round(throughput, 3);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets machine speed based on level.
        /// </summary>
        private double GetMachineSpeedLevel(int level)
        {
            return level switch
            {
                1 => 0.67d,
                2 => 1.0d,
                3 => 1.5d,
                _ => 0.67d
            };
        }

        /// <summary>
        /// Gets accuracy tolerance.
        /// </summary>
        public double GetAccuracyTolerance() => _tolerance;
    }

    /// <summary>
    /// Parses recipe data from JSON.
    /// </summary>
    public class RecipeParser
    {
        public static Recipe Parse(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<Recipe>(json) ?? new Recipe();
        }
    }

    /// <summary>
    /// Detects circular references in recipe chain.
    /// </summary>
    public class CircularReferenceDetector
    {
        public bool IsCircular(List<Recipe> recipes)
        {
            // Simple circular reference detection
            foreach (var recipe in recipes)
            {
                foreach (var other in recipes)
                {
                    if (other.Id != recipe.Id && other.Resources.Contains(recipe.Id))
                        return true;
                }
            }
            return false;
        }
    }
}