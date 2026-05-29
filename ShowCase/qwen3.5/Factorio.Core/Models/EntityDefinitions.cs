using System;
using System.Collections.Generic;

namespace Factorio.Core.Models
{
    /// <summary>
    /// Represents a complete factory save entity.
    /// </summary>
    public class FactorySave
    {
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public int SaveId { get; set; }
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<Machine> Machines { get; set; } = new List<Machine>();
        public ICollection<Node> Nodes { get; set; } = new List<Node>();
        public ICollection<Edge> Edges { get; set; } = new List<Edge>();
        public string? Path { get; set; }
        public double TotalThroughput { get; set; }
        public Bottleneck? Bottleneck { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
