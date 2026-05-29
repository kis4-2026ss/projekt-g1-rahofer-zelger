// DomainModels.cs
// Factorio Modeler Engine Core - Domain Models
// Revision 4.0.0

using System;

namespace Factorio.Engine.Models
{
    /// <summary>
    /// Domain model for Factorio recipe calculations.
    /// </summary>
    public class ThroughputModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string OutputItemId { get; set; }
        public string MachineType { get; set; }
        public string MachineEmoji { get; set; }
        public int OutputQty { get; set; }
        public double CraftingTime { get; set; }
        public double MachineSpeed { get; set; }
        public double CalculatedThroughput { get; set; }
        public double AccuracyTolerance { get; set; } = 0.001;
        public int MinutesPerHour { get; set; } = 60;
        public double BaseRate { get; set; }
        public double MultiMachineCount { get; set; } = 1;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public bool IsValidated { get; set; }
    }

    /// <summary>
    /// Resource requirement for a recipe.
    /// </summary>
    public class ResourceRequirement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ItemId { get; set; }
        public int Amount { get; set; }
        public int Minimutely { get; set; }
    }

    /// <summary>
    /// Machine speed tier definition.
    /// </summary>
    public class MachineSpeedTier
    {
        public int Tier { get; set; }
        public string MachineTypeName { get; set; }
        public double SpeedMultiplier { get; set; }
        public string Category { get; set; }
        public string CategoryName { get; set; }
    }
}
