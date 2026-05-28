using System;
using System.Collections.Generic;

namespace Engine.Calculator
{
    /// <summary>
    /// Represents a recipe with output quantity, crafting time, and resource requirements.
    /// </summary>
    public class Recipe
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int OutputQty { get; set; }
        public int CraftingTime { get; set; }
        public List<Resource> Resources { get; set; }
        public string? MachineType { get; set; }
        public string? CircuitRequirement { get; set; }

        public static Recipe Create(string id, int outputQty, int craftingTime, List<Resource> resources)
        {
            return new Recipe
            {
                ID = id,
                Name = id,
                OutputQty = outputQty,
                CraftingTime = craftingTime,
                Resources = resources ?? new List<Resource>()
            };
        }
    }

    /// <summary>
    /// Represents a resource required for a recipe.
    /// </summary>
    public class Resource
    {
        public string Type { get; set; }
        public int Amount { get; set; }
        public string Slot { get; set; }
    }

    /// <summary>
    /// Express Splitter for splitting resource flow.
    /// </summary>
    public class ExpressSplitter
    {
        public string ID { get; set; }
        public int OutputQty { get; set; }
        public int CraftingTime { get; set; }
        public List<Resource> Resources { get; set; }
    }

    /// <summary>
    /// Validation result for calculation operations.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public double Error { get; set; }
        public double Accuracy { get; set; }

        public static ValidationResult Valid => new ValidationResult
        {
            IsValid = true,
            ErrorMessage = null,
            Error = 0,
            Accuracy = 100.0
        };

        public static ValidationResult Invalid(string message, double expected = 0, double actual = 0)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = message,
                Error = Math.Abs(actual - expected),
                Accuracy = CalculateAccuracy(expected, actual)
            };
        }

        private static double CalculateAccuracy(double expected, double actual)
        {
            if (expected <= 0) return 0;
            return (1.0 - Math.Abs(actual - expected) / expected) * 100;
        }
    }
}