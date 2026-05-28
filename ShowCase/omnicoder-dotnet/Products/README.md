using FactorioModeler.Models;

namespace FactorioModeler.Products;

/// <summary>
/// Represents a machine in the Factorio simulation.
/// 
/// Machines in Factorio Modeler are production entities that process
/// resources according to recipes. Each machine has a specific type
/// along with speed multipliers and output limits.
/// </summary>
public class Machine
{
    /// <summary>
    /// Unique identifier for the machine type.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the machine.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Technical machine type (e.g., "assembling-machine-2").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Emoji icon representing the machine.
    /// </summary>
    public string Emoji { get; set; } = string.Empty;

    /// <summary>
    /// Minimum speed multiplier the machine can operate at.
    /// </summary>
    public double MinSpeed { get; set; }

    /// <summary>
    /// Maximum speed multiplier the machine can operate at.
    /// </summary>
    public double MaxSpeed { get; set; }

    /// <summary>
    /// Category of the machine (industrial, belt, storage, etc.).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of output slots for this machine.
    /// </summary>
    public int MaxOutputs { get; set; }

    /// <summary>
    /// Color code for category visualization.
    /// </summary>
    public string CategoryColor { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable category name (e.g., "Machine", "Belt").
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Base production speed when running at max speed.
    /// </summary>
    public double BaseSpeed { get; set; }

    /// <summary>
    /// Whether this machine is operational.
    /// </summary>
    public bool Operational { get; set; } = true;

    /// <summary>
    /// Get current effective speed.
    /// </summary>
    public double EffectiveSpeed => MinSpeed > 0 ? (this.Operational ? MinSpeed : MinSpeed * MaxSpeed) : 0;

    /// <summary>
    /// Set minimum and maximum speed.
    /// </summary>
    /// <param name="minSpeed">Minimum speed multiplier.</param>
    /// <param name="maxSpeed">Maximum speed multiplier.</param>
    public void SetSpeedRange(double minSpeed, double maxSpeed)
    {
        this.MinSpeed = minSpeed;
        this.MaxSpeed = maxSpeed;
    }

    /// <summary>
    /// Get current production state.
    /// </summary>
    public MachineState GetState()
    {
        return this.Operational && (this.MaxSpeed - this.MinSpeed) > 0 ? MachineState.Running : MachineState.Stopped;
    }
}

/// <summary>
/// Machine state enumeration.
/// </summary>
public enum MachineState
{
    Idle,
    Running,
    Stopped,
    Overheating,
    Broken
}

/// <summary>
/// Product entity type alias.
/// </summary>
public class MachineProduct : Product
{
    /// <summary>
    /// Associated machine instance.
    /// </summary>
    public Machine Machine { get; set; } = default!;
}