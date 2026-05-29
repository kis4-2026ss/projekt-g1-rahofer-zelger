using System;
using System.Text.Json.Serialization;

namespace FactorioModeler.Models.Speed
{
    /// <summary>
    /// Represents speed tier configurations for different machine types.
    /// </summary>
    [Serializable]
    public class SpeedTier
    {
        /// <summary>
        /// Gets or sets the machine type identifier.
        /// </summary>
        [JsonPropertyName("machineType")]
        public string MachineType { get; set; }

        /// <summary>
        /// Gets or sets the speed multiplier value.
        /// </summary>
        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        /// <summary>
        /// Initializes a new instance of the SpeedTier class.
        /// </summary>
        /// <param name="machineType">
        /// The machine type identifier.</param>
        /// <param name="speed">
        /// The speed multiplier.</param>
        public SpeedTier(string machineType, double speed)
        {
            MachineType = machineType;
            Speed = speed;
        }

        /// <summary>
        /// Initializes a new instance of the SpeedTier class.
        /// </summary>
        public SpeedTier()
        {
        }
    }

    /// <summary>
    /// Represents a collection of speed tier mappings for all machine types.
    /// </summary>
    [Serializable]
    public class SpeedTierCollection
    {
        /// <summary>
        /// Gets or sets the speed tier entries.
        /// </summary>
        [JsonPropertyName("tiers")]
        public Dictionary<string, SpeedTier> Tiers { get; set; }

        /// <summary>
        /// Initializes a new instance of the SpeedTierCollection class.
        /// </summary>
        public SpeedTierCollection()
        {
            Tiers = new Dictionary<string, SpeedTier>();
        }

        /// <summary>
        /// Adds a new speed tier entry.
        /// </summary>
        /// <param name="machineType">
        /// The machine type.</param>
        /// <param name="speed">
        /// The speed multiplier.</param>
        public void Add(string machineType, double speed)
        {
            if (!Tiers.ContainsKey(machineType))
            {
                Tiers[machineType] = new SpeedTier(machineType, speed);
            }
            else
            {
                Tiers[machineType].Speed = speed;
            }
        }

        /// <summary>
        /// Retrieves a speed tier by machine type.
        /// </summary>
        /// <param name="machineType">
        /// The machine type to retrieve.</param>
        /// <returns>
        /// The SpeedTier or null if not found.</returns>
        public SpeedTier GetSpeedTier(string machineType)
        {
            return Tiers.TryGetValue(machineType, out var tier) ? tier : null;
        }

        /// <summary>
        /// Gets or sets the default base speed value.
        /// </summary>
        [JsonPropertyName("baseSpeed")]
        public double BaseSpeed { get; set; } = 1.0;
    }
}
