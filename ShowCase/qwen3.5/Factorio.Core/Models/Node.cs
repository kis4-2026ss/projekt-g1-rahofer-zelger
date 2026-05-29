namespace Factorio.Core.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a node in the production chain graph.
/// Each node corresponds to a machine or resource point.
/// </summary>
public class Node
{
    public string NodeId { get; set; } = "";
    public string ItemType { get; set; } = ""; // Material type produced/consumed
    public string MachineType { get; set; } = "";
    public string IconReference { get; set; } = "";
    public double Throughput { get; set; } // Items per minute
    public bool IsInput { get; set; }
    public bool IsOutput { get; set; }

    /// <summary>
    /// Creates a new production node
    /// </summary>
    /// <param name="nodeId">Unique identifier for the node</param>
    /// <param name="itemType">Type of item this machine produces/consumes</param>
    /// <param name="machineType">Type of machine (e.g., "furnace", "assembling-machine")</param>
    /// <param name="throughput">Items produced per minute</param>
    /// <returns>New Node instance</returns>
    public static Node Create(string nodeId, string itemType, string machineType, double throughput)
    {
        return new Node
        {
            NodeId = nodeId,
            ItemType = itemType,
            MachineType = machineType,
            Throughput = throughput,
            IsInput = false,
            IsOutput = false
        };
    }

    /// <summary>
    /// Identifies if this node is a graph input (upstream resource)
    /// </summary>
    public bool IsUpstream()
    {
        return IsInput;
    }

    /// <summary>
    /// Identifies if this node is a graph output (final product)
    /// </summary>
    public bool IsDownstream()
    {
        return IsOutput;
    }
}
