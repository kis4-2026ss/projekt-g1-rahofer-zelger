using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FactorioModeler.Models.Recipes
{
    /// <summary>
    /// Represents a crafting recipe with resource requirements and output specifications.
    /// </summary>
    [Serializable]
    public class Recipe
    {
        /// <summary>
        /// Gets or sets the unique identifier for the recipe.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of machine that executes this recipe.
        /// </summary>
        [JsonPropertyName("machineType")]
        public string MachineType { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the recipe.
        /// </summary>
        [JsonPropertyName("recipeName")]
        public string RecipeName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the output material.
        /// </summary>
        [JsonPropertyName("outputItemId")]
        public string OutputItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity of output produced per crafting run.
        /// </summary>
        [JsonPropertyName("outputQty")]
        public int OutputQty { get; set; }

        /// <summary>
        /// Gets or sets the duration of crafting in seconds.
        /// </summary>
        [JsonPropertyName("craftingTime")]
        public int CraftingTime { get; set; }

        /// <summary>
        /// Gets or sets the list of required input resources.
        /// </summary>
        [JsonPropertyName("requiredResources")]
        public List<RequiredResource> RequiredResources { get; set; }

        /// <summary>
        /// Gets or sets the throughput rate per machine speed.
        /// </summary>
        [JsonPropertyName("outputRate")]
        public int OutputRate { get; set; }

        /// <summary>
        /// Computes the total resource consumption per production cycle.
        /// </summary>
        public Dictionary<string, int> ComputeTotalResourceConsumption()
        {
            if (RequiredResources == null) return new Dictionary<string, int>();

            var consumption = new Dictionary<string, int>();
            foreach (var resource in RequiredResources)
            {
                if (!consumption.ContainsKey(resource.ItemId))
                {
                    consumption[resource.ItemId] = 0;
                }
                consumption[resource.ItemId] += resource.Amount;
            }
            return consumption;
        }

        /// <summary>
        /// Calculates effective throughput based on machine speed.
        /// </summary>
        /// <param name="machineSpeed">
The speed multiplier of the executing machine.</param>
        /// <returns>The effective output rate per second.</returns>
        public double CalculateEffectiveThroughput(double machineSpeed)
        {
            double cyclesPerSecond = machineSpeed / CraftingTime;
            return OutputQty * cyclesPerSecond;
        }
    }
}
