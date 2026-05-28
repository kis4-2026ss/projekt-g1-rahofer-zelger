using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioModeler.Engine
{
    public class Recipe
    {
        public string Id { get; set; } = string.Empty;
        public string MachineType { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public string OutputItemId { get; set; } = string.Empty;
        public int OutputQty { get; set; }
        public int CraftingTime { get; set; }
        public List<ResourceRequirement> RequiredResources { get; set; } = new();
        public double OutputRate { get; set; }
    }

    public class ResourceRequirement
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int Minutely { get; set; }
    }

    public class Machine
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public int MinSpeed { get; set; }
        public int MaxSpeed { get; set; }
        public string Category { get; set; } = string.Empty;
        public int MaxOutputs { get; set; }
        public string CategoryColor { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public double BaseSpeed { get; set; }
    }

    public class SpeedTier
    {
        public string MachineType { get; set; } = string.Empty;
        public double Speed { get; set; }
    }
}