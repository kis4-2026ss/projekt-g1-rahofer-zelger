using System;
using System.Text.Json.Serialization;

namespace FactorioModeler.Models.Resources
{
    /// <summary>
    /// Represents a required resource for a recipe or machine operation.
    /// </summary>
    [Serializable]
    public class RequiredResource
    {
        /// <summary>
        /// Gets or sets the unique material identifier.
        /// </summary>
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity required per crafting run.
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the per-minute throughput rate.
        /// </summary>
        [JsonPropertyName("minutely")]
        public int Minutely { get; set; }

        /// <summary>
        /// Initializes a new instance of the RequiredResource class.
        /// </summary>
        /// <param name="itemId">
        The item identifier.</param>
        /// <param name="amount">
        The required amount.</param>
        /// <param name="minutely">
        The per-minute throughput rate.</param>
        public RequiredResource(string itemId, int amount, int minutely)
        {
            ItemId = itemId;
            Amount = amount;
            Minutely = minutely;
        }

        /// <summary>
        /// Initializes a new instance of the RequiredResource class.
        /// </summary>
        public RequiredResource()
        {
        }

        /// <summary>
        /// Compares two RequiredResource instances for equality.
        /// </summary>
        /// <param name="other">
        /// The object to compare.</param>
        /// <returns>
        /// True if equal.</returns>
        public bool Equals(RequiredResource other)
        {
            if (other == null) return false;
            return ItemId == other.ItemId && Amount == other.Amount;
        }

        /// <summary>
        /// Gets the hash code for this resource.
        /// </summary>
        /// <returns>
        /// The hash code.</returns>
        public override int GetHashCode()
        {
            return ItemId?.GetHashCode() ^ Amount.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the resource.
        /// </summary>
        public override string ToString()
        {
            return $"{ItemId} ({Amount} units)";
        }
    }
}
