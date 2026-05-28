using System.Collections.Generic;
using Factorio.Core.Models;

namespace Factorio.Core.Services;

/// <summary>
/// Production chain analyzer
/// </summary>
public class ProductionChainAnalyzer
{
    /// <summary>
    /// Analyze production chain for specific item
    /// </summary>
    public AnalysisResult AnalyzeChain(string itemName, FactorySave factory)
    {
        var upstream = new List<AnalysisItem>();
        var downstream = new List<AnalysisItem>();
        double netRate = 0;

        // Simplified chain analysis
        foreach (var machine in factory.Machines)
        {
            var rate = Math.Max(0, machine.ProductionRate - machine.CraftingQueue);
            upstream.Add(new AnalysisItem
            {
                ItemId = factory.ItemCounts.FindIndex(x => x.Item1.ToString() == machine.ItemName)
                ItemName = machine.ItemName,
                Rate = rate
            
        }

        return new AnalysisResult
        {
            ItemName = itemName,
            UpstreamCount = upstream.Count,
            DownstreamCount = downstream.Count,
            NetRate = netRate
        };
    }

}
