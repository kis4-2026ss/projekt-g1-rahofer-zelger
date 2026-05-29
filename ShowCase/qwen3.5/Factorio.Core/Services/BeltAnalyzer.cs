using Facilityo.Core.Models;

namespace Factorio.Core.Services
{
    /// <summary>
    /// Analyzes belt capacity and saturation.
    /// </summary>
    public class BeltAnalyzer
    {
        /// <summary>
        /// Calculates belt analysis for a list of belts.
        /// </summary>
        /// <param name="belts">List of belt entities.</param>
        /// <param name="items">Items on belts.</param>
        /// <returns>Band analysis result.</returns>
        public BandAnalysis CalculateBeltAnalysis(List<BeltEntity> belts, List<ItemQuantity> items)
        {
            var analysis = new BandAnalysis();
            var beltDictionary = new Dictionary<string, BeltEntity>();

            foreach (var belt in belts)
            {
                beltDictionary[belt.Id] = belt;
            }

            foreach (var item in items)
            {
                var belt = GetBeltForItem(beltDictionary, item.Material);
                if (belt != null)
                {
                    item.Position = Math.Min(item.Position, belt.BeltSize);
                    item.QualityLevel = item.Material == "log" ? "wood" : "plasteel";
                }
            }

            analysis.Satisfaction = items.Count > 0 ? 0.0 : 0.0;
            return analysis;
        }

        private BeltEntity? GetBeltForItem(Dictionary<string, BeltEntity> belts, string item)
        {
            if (!belts.ContainsKey(item)) return null;
            return belts[item];
        }
    }

    /// <summary>
    /// Represents belt analysis result.
    /// </summary>
    public class BandAnalysis
    {
        public double Satisfaction { get; set; }
        public int ItemsOnBelt { get; set; }
        public int BeltSize { get; set; }
        public string QualityLevel { get; set; } = "";
    }

    /// <summary>
    /// Belt entity definition.
    /// </summary>
    public class BeltEntity
    {
        public string Id { get; set; } = "";
        public string BeltType { get; set; } = "";
        public int BeltSize { get; set; } = 15;
        public int MaxCapacity { get; set; } = 50;
        public double Saturation { get; set; } = 0.0;
        public string Position { get; set; } = "";
    }

    /// <summary>
    /// Item quantity on belt.
    /// </summary>
    public class ItemQuantity
    {
        public string Material { get; set; } = "";
        public int Position { get; set; }
        public string QualityLevel { get; set; } = "";
    }
}
