using FactorioModeler.Models;

namespace FactorioModeler.Products;

/// <summary>
/// Represents a machine in the Factorio Modeler simulation.
///
/// Machines are production entities that consume resources and produce outputs
/// based on configured recipes. Each machine has a unique identifier, speed
/// tiers, operating limits, and category classifications.
///
/// Key Properties:
/// - Id: Unique identifier for the machine type (e.g., "assembling-machine-2")
/// - Name: Human-readable name of the machine
/// - Type: Technical machine type used by the Factorio API
/// - MinSpeed/MaxSpeed: Speed multiplier range (0-1)
/// - BaseSpeed: Production speed at maximum speed tier
/// - Category: Classification (industrial, belt, storage, etc.)
/// - MaxOutputs: Number of product slots available
///
/// Usage:
/// Each machine instance represents a single machine entity in the simulation.
/// Production calculations use the BaseSpeed multiplied by the effective speed.
/// Machines can be paused (MinSpeed < MaxSpeed) or run at full capacity.
/// </summary>
public class Machine
{
    /// <summary>
    /// Unique identifier for this machine type.
    /// </summary>
    public string Id { get; set; } = "assembling-machine-2";

    /// <summary>
    /// Human-readable display name.
    /// </summary>
    public string Name { get; set; } = "Assembling Machine Level 2";

    /// <summary>
    /// Technical machine type for API integration.
    /// </summary>
    public string Type { get; set; } = "assembling-machine-2";

    /// <summary>
    /// Emoji icon for visual representation.
    /// </summary>
    public string Emoji { get; set; } = "\uD83C\uDFE0";

    /// <summary>
    /// Minimum speed multiplier (0 = stopped, 1 = running).
    /// </summary>
    public double MinSpeed { get; set; } = 0;

    /// <summary>
    /// Maximum speed multiplier.
    /// </summary>
    public double MaxSpeed { get; set; } = 10;

    /// <summary>
    /// Category classification.
    /// </summary>
    public string Category { get; set; } = "industrial";

    /// <summary>
    /// Maximum number of output slots.
    /// </summary>
    public int MaxOutputs { get; set; } = 10;

    /// <summary>
    /// Color code for category visualization.
    /// </summary>
    public string CategoryColor { get; set; } = "red";

    /// <summary>
    /// Category name for display.
    /// </summary>
    public string CategoryName { get; set; } = "Machine";

    /// <summary>
    /// Base production speed at maximum speed.
    /// </summary>
    public double BaseSpeed { get; set; } = 1.0;

    /// <summary>
    /// Whether this machine is operational.
    /// </summary>
    public bool Operational { get; set; } = true;

    /// <summary>
    /// Current production count.
    /// </summary>
    public int ProductionCount { get; set; } = 0;

    /// <summary>
    /// Calculate effective speed based on operational state.
    /// </summary>
    public double EffectiveSpeed => this.Operational ? this.MaxSpeed * BaseSpeed : 0;

    /// <summary>
    /// Set speed range for the machine.
    /// </summary>
    public void SetSpeed(double min = 0, double max = 10)
    {
        this.MinSpeed = min;
        this.MaxSpeed = max;
    }
}

/// <summary>
/// Represents a product entity in the simulation.
///
/// Products are the core elements tracked in the Factorio Modeler:
/// - Materials: Raw inputs (iron plates, copper cables)
/// - Components: Intermediate products (steel plates)
/// - Machines: Production equipment (assemblers, furnaces)
///
/// All products inherit from Product base class.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for this product.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Current quantity in simulation.
    /// </summary>
    public double Quantity { get; set; }
}

/// <summary>
/// Represents a machine product in the simulation.
///
/// Machine products represent the equipment entities that
/// perform production operations.
/// </summary>
public class MachineProduct : Product
{
    /// <summary>
    /// Associated machine configuration.
    /// </summary>
    public Machine Machine { get; set; } = default!;

    /// <summary>
    /// Gets the product type.
    /// </summary>
    public override ProductType Type => ProductType.Machine;
}

/// <summary>
/// Represents a machine product with additional context.
/// </summary>
public class MachineContext
{
    /// <summary>
    /// Machine instance.
    /// </summary>
    public Machine Machine { get; set; } = default!;

    /// <summary>
    /// Current production recipe.
    /// </summary>
    public Recipe? CurrentRecipe { get; set; }

    /// <summary>
    /// Available output slots.
    /// </summary>
    public List<cProduct> outputs => new();

    /// <summary>
    /// Calculate current production rate.
    /// </summary>
    public double ProductionRate
    {
        get
        {
            if (this.CurrentRecipe is null) return 0;
            return this.CurrentRecipe.OutputQty * this.Machine.EffectiveSpeed;
        }
    }
}