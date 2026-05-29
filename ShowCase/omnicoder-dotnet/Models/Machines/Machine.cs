using System;
using System.Text.Json.Serialization;

namespace FactorioModeler.Models.Machines
{
    /// <summary>
    /// Represents an industrial machine with speed tiers and output capabilities.
    /// </summary>
    [Serializable]
    public class Machine
    {
        /// <summary>
        /// Gets or sets the unique machine identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the human-readable machine name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the machine type identifier.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the emoji icon representation.
        /// </summary>
        [JsonPropertyName("emoji")]
        public string Emoji { get; set; }

        /// <summary>
        /// Gets or sets the minimum operational speed.
        /// </summary>
        [JsonPropertyName("minSpeed")]
        public double MinSpeed { get; set; }

        /// <summary>
        /// Gets or sets the maximum operational speed.
        /// </summary>
        [JsonPropertyName("maxSpeed")]
        public double MaxSpeed { get; set; }

        /// <summary>
        /// Gets or sets the machine category.
        /// </summary>
        [JsonPropertyName("category")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the maximum output slots.
        /// </summary>
        [JsonPropertyName("maxOutputs")]
        public int MaxOutputs { get; set; }

        /// <summary>
        /// Gets or sets the category color code.
        /// </summary>
        [JsonPropertyName("categoryColor")]
        public string CategoryColor { get; set; }

        /// <summary>
        /// Gets or sets the category display name.
        /// </summary>
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the base speed multiplier.
        /// </summary>
        [JsonPropertyName("baseSpeed")]
        public double BaseSpeed { get; set; }
    }
}
