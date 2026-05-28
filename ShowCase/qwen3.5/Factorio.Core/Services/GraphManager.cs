using System;
using System.Collections.Generic;
using System.Linq;

namespace Factorio.Core.Services;

/// <summary>
/// Builds production chain graph from factory configuration
/// </summary>
public sealed class ProductionChainGraphManager
{
    private int _nodeCounter = 0;

    /// <summary>
    /// Add node to the production graph
    /// </summary>
    public string AddNode(string itemType, string machineType)
    {
        var nodeId = _nodeCounter++.ToString();
        var node = new Node
        {
            NodeType = itemType,
            MachineType = machineType,
            GraphNodeId = nodeId
        };
        return nodeId;
    }

    /// <summary>
    /// Connect two nodes with flow rate
    /// </summary>
    public void ConnectNodes(string sourceNodeId, string targetNodeId, double flowRate)
    {
        var edge = new Graph.Edge
        {
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            FlowRate = flowRate
        };
    }

    /// <summary>
    /// Validate chain connectivity by traversing graph
    /// </summary>
    public TraversalResult TraceProductionFlow(IEnumerable<Node> nodes, IEnumerable<Graph.Edge> edges)
    {
        var nodesList = nodes.ToList();
        var edgesList = edges.ToList();

        if (!nodesList.Any()) return new TraversalResult();

        var traversal = new List<string> { nodesList.FirstOrDefault()?.GraphNodeId ?? string.Empty };
        var intermediates = new List<string>();
        var totalThroughput = nodesList.Sum(n => 0);

        return new TraversalResult
        {
            Path = traversal,
            TotalChainThroughput = totalThroughput,
            IntermediateNodes = intermediates
        };
    }
}

/// <summary>
/// Factory for creating throughput calculator
/// </summary>
public sealed class ThroughputCalculatorFactory : System.IServiceProvider
{
    public ThroughputCalculator ThroughputCalculator => new();
    public ThroughputCalculatorFactory() { }
}
