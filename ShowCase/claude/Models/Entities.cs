using FactorioModeler.Products;

namespace FactorioModeler.Models;

/// <summary>
/// Represents a general production entity in the Factorio simulation.
/// 
/// Production entities are the core building blocks of the factory simulation.
/// They include products, resources, machines, and recipes that interact
/// during production cycles.
/// </summary>
public class ProductionEntity
{
    /// <summary>
    /// Unique identifier for this production entity.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of production entity (Material, Component, Machine, etc.).
    /// </summary>
    public ProductType EntityType { get; set; }

    /// <summary>
    /// Current production state of this entity.
    /// </summary>
    public ProductionState State { get; set; } = ProductionState.Idle;

    /// <summary>
    /// Quantity produced/consumed.
    /// </summary>
    public double Quantity { get; set; }

    /// <summmary>
    /// Production rate (items per minute).
    /// </summmary>
    public double ProductionRate { get; set; }

    /// <summary>
    /// Gets or sets associated recipe.
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// Gets or sets associated machine.
    /// </summary>
    public Machine? Machine { get; set; }
}

/// <summary>
/// Enum representing production states.
/// </summary>
public enum ProductionState
{
    Idle,
    Running,
    Stopped,
    Broken,
    Crafting
}

/// <summary>
/// Collection of all production entities in the simulation.
/// </summary>
public class ProductionEntities
{
    /// <summary>
    /// Gets all material entities.
    /// </summary>
    public Dictionary<string, MaterialProduct> Materials { get; set; } = new();

    /// <summary>
    /// Gets all component entities.
    /// </summary>
    public Dictionary<string, ComponentProduct> Components { get; set; } = new();

    /// <summary>
    /// Gets all machine entities.
    /// </summary>
    public Dictionary<string, MachineProduct> Machines { get; set; } = new();

    /// <summary>
    /// Gets all resource entities.
    /// </summary>
    public Dictionary<string, ResourceProduct> Resources { get; set; } = new();

    /// <summary>
    /// Gets all recipe entities.
    /// </summary>
    public Dictionary<string, RecipeProduct> Recipes { get; set; } = new();

    /// <summary>
    /// Get all entities by type.
    /// </summary>
    /// <param name="type">Product type to filter by.</param>
    public IEnumerable<Product> GetByType(ProductType type)
    {
        foreach (var (_, product) in Materials) yield return product;
        foreach (var (_, product) in Components) yield return product;
        foreach (var (_, product) in Machines) yield return product;
        foreach (var (_, product) in Resources) yield return product;
        foreach (var (_, product) in Recipes) yield return product;
    }

    /// <summary>
    /// Add a production entity.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    public void Add(Product entity)
    {
        if (entity is MaterialProduct mp) Materials.Add(mp.Key, mp);
        if (entity is ComponentProduct cp) Components.Add(cp.Key, cp);
        if (entity is MachineProduct mpp) Machines.Add(mpp.Key, mpp);
        if (entity is ResourceProduct rp) Resources.Add(rp.Key, rp);
        if (entity is RecipeProduct rpp) Recipes.Add(rpp.Key, rpp);
    }
}

/// <summary>
/// Represents the overall production graph for the factory.
/// 
/// ProductionGraph models the dependency relationships between
/// different product types, recipes, and machines in the simulation.
/// </summary>
public class ProductionGraph
{
    /// <summary>
    /// Directed graph representing production dependencies.
    /// 
    /// Vertices: Products (Materials, Components, Machines, Resources, Recipes)
    /// Edges: "consumes" (input) and "produced-by" (output) relationships
    /// </summary>
    private readonly Graph<Vertex> _vertices = new();

    /// <summary>
    /// Get all vertices (entities) in the graph.
    /// </summary>
    public IEnumerable<Vertex> Vertices => _vertices.Vertices;

    /// <summary>
    /// Add a vertex representing a production entity.
    /// </summary>
    public void AddVertex(string id, string name)
    {
        _vertices.AddVertex(id, name);
    }

    /// <summary>
    /// Add an edge representing a dependency.
    /// </summary>
    public void AddEdge(string from, string to, EdgeType type)
    {
        _vertices.Edge(from, to, type);
    }

    /// <summary>
    /// Get all dependencies for a given entity.
    /// </summary>
    public IEnumerable<string> GetDependencies(string entity)
    {
        foreach (var (from, to, type) in _vertices.Edges)
        {
            if (from == entity) yield return to;
            if (to == entity) yield return from;
        }
    }

    /// <summary>
    /// Detect circular dependencies in the production chain.
    /// </summary>
    public bool HasCircularDependency(string startNode)
    {
        return _vertices.HasCycle(startNode);
    }

    /// <summary>
    /// Validate the entire graph for cycles.
    /// </summary>
    public bool Validate()
    {
        foreach (var vertex in _vertices.Vertices)
        {
            if (_vertices.HasCycle(vertex.Id))
                return false;
        }
        return true;
    }
}

/// <summary>
/// Graph vertex definition for production graph.
/// </summary>
public class Vertex
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Edge type for production graph.
/// </summary>
public enum EdgeType
{
    ConsumedBy,
    ProducedBy,
    Requires
}

/// <summary>
/// Simple directed graph data structure.
/// </summary>
public class Graph<T>
{
    public readonly Dictionary<string, Vertex<T>> Vertices = new();
    public readonly Dictionary<string, List<Relationship>> Edges = new();

    public void AddVertex(string id, string name)
    {
        Vertices[id] = new Vertex<T> { Id = id, Name = name };
        Edges[id] = new List<Relationship>();
    }

    public void Edge(string from, string to, EdgeType type)
    {
        if (!Vertices.ContainsKey(from)) Vertices[from] = new Vertex<T> { Id = from };
        if (!Vertices.ContainsKey(to)) Vertices[to] = new Vertex<T> { Id = to };
        Edges[from].Add(new Relationship { Target = to, Type = type });
    }

    public bool HasCycle(string startNode)
    {
        HashSet<string> visited = new();
        HashSet<string> recursionStack = new();

        Stack<string> stack = new();
        stack.Push(startNode);

        while (stack.Count > 0)
        {
            string node = stack.Pop();

            if (recursionStack.Contains(node)) return true;
            if (visited.Contains(node)) continue;

            visited.Add(node);
            recursionStack.Add(node);

            if (!Edges.ContainsKey(node)) continue;

            foreach (var edge in Edges[node])
            {
                if (edge.Target == startNode) return true;

                if (Edges.TryGetValue(edge.Target, out var targetEdges))
                {
                    foreach (var targetEdge in targetEdges)
                    {
                        if (targetEdge.Target == node) return true;
                    }
                }

                if (vertices.TryGetValue(edge.Target, out var targetVertices))
                {
                    foreach (var targetVertex in targetVertices)
                    {
                        if (targetVertex.Id == startNode) return true;
                    }
                }
            }
            recursionStack.Remove(node);
        }

        return false;
    }

    private Vertex<T> vertices => Vertices;

    public class Vertex<T>
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public T Data { get; set; } = default!;
    }

    private class Relationship
    {
        public string Target { get; set; } = string.Empty;
        public EdgeType Type { get; set; }
    }
}

/// <summary>
/// Extended vertex type for production graph nodes.
/// 
/// Each node wraps a Product entity with metadata.
/// </summary>
public class Vertex
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Product? Product { get; set; }
}

/// <summary>
/// Edge representation for production graph.
/// 
/// Edges represent input/output relationships between products,
/// recipes, and machines.
/// </summary>
public class Edge
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public EdgeType Type { get; set; } = EdgeType.ConsumedBy;
    public double Weight { get; set; } = 1.0;
}