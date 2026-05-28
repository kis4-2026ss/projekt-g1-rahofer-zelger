using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FactorioModeler.Models.Simulation
{
    /// <summary>
    /// Represents the current state of a single machine during simulation.
    /// </summary>
    [Serializable]
    public class MachineState
    {
        /// <summary>
        /// Gets or sets the machine ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets whether the machine is active.
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the current production efficiency.
        /// </summary>
        [JsonPropertyName("efficiency")]
        public double Efficiency { get; set; }

        /// <summary>
        /// Gets or sets any errors or warnings for this machine.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the current output count.
        /// </summary>
        [JsonPropertyName("outputs")]
        public int Outputs { get; set; }

        /// <summary>
        /// Gets the status string for display.
        /// </summary>
        public string Status
        {
            get
            {
                if (IsActive)
                    return $"Running (Efficiency: {Efficiency:F0}%)";
                return "Stopped";
            }
        }

        /// <summary>
        /// Initializes a new instance of the MachineState class.
        /// </summary>
        public MachineState()
        {
            IsActive = true;
            Efficiency = 90.0;
        }
    }

    /// <summary>
    /// Represents buffer/storage state for materials.
    /// </summary>
    [Serializable]
    public class BufferState
    {
        /// <summary>
        /// Gets or sets the material ID.
        /// </summary>
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the current buffer level.
        /// </summary>
        [JsonPropertyName("level")]
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the maximum buffer capacity.
        /// </summary>
        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        /// <summary>
        /// Gets the utilization percentage.
        /// </summary>
        public double Utilization
        {
            get
            {
                if (Capacity <= 0) return 0.0;
                return (Level / (double)Capacity) * 100.0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the BufferState class.
        /// </summary>
        public BufferState()
        {
            Level = 0;
            Capacity = 100;
        }
    }

    /// <summary>
    /// Represents belt state for material transport.
    /// </summary>
    [Serializable]
    public class BeltState
    {
        /// <summary>
        /// Gets or sets the belt ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the materials currently on the belt.
        /// </summary>
        [JsonPropertyName("materials")]
        public List<Material> Materials { get; set; }

        /// <summary>
        /// Gets or sets whether the belt is active.
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the belt speed.
        /// </summary>
        [JsonPropertyName("speed")]
        public int Speed { get; set; }

        /// <summary>
        /// Initializes a new instance of the BeltState class.
        /// </summary>
        public BeltState()
        {
            Materials = new List<Material>();
            IsActive = true;
            Speed = 1;
        }
    }

    /// <summary>
    /// Represents a material currently on a belt.
    /// </summary>
    [Serializable]
    public class Material
    {
        /// <summary>
        /// Gets or sets the material ID.
        /// </summary>
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity on this belt segment.
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Initializes a new instance of the Material class.
        /// </summary>
        public Material()
        {
        }
    }

    /// <summary>
    /// Represents the simulation state including time step and factory-wide state.
    /// </summary>
    [Serializable]
    public class SimulationState
    {
        /// <summary>
        /// Gets or sets the current simulation time step.
        /// </summary>
        [JsonPropertyName("timeStep")]
        public int TimeStep { get; set; }

        /// <summary>
        /// Gets or sets the total elapsed simulation time in seconds.
        /// </summary>
        [JsonPropertyName("elapsedTime")]
        public double ElapsedTime { get; set; }

        /// <summary>
        /// Gets or sets the overall factory efficiency.
        /// </summary>
        [JsonPropertyName("efficiency")]
        public double Efficiency { get; set; }

        /// <summary>
        /// Gets or sets the current belt states.
        /// </summary>
        [JsonPropertyName("belts")]
        public Dictionary<string, BeltState> Belts { get; set; }

        /// <summary>
        /// Gets or sets the current buffer states.
        /// </summary>
        [JsonPropertyName("buffers")]
        public Dictionary<string, BufferState> Buffers { get; set; }

        /// <summary>
        /// Gets or sets the current machine states.
        /// </summary>
        [JsonPropertyName("machines")]
        public Dictionary<string, MachineState> Machines { get; set; }

        /// <summary>
        /// Gets or sets resource stock levels.
        /// </summary>
        [JsonPropertyName("resources")]
        public Dictionary<string, int> Resources { get; set; }

        /// <summary>
        /// Gets or sets current production queue.
        /// </summary>
        [JsonPropertyName("queue")]
        public List<string> Queue { get; set; }

        /// <summary>
        /// Initializes a new instance of the SimulationState class.
        /// </summary>
        public SimulationState()
        {
            Belts = new Dictionary<string, BeltState>();
            Buffers = new Dictionary<string, BufferState>();
            Machines = new Dictionary<string, MachineState>();
            Resources = new Dictionary<string, int>();
            Queue = new List<string>();
        }

        /// <summary>
        /// Advances the simulation by one time step.
        /// </summary>
        /// <returns>
        /// True if simulation advanced successfully.
        /// </returns>
        public bool Advance()
        {
            TimeStep++;
            ElapsedTime++;
            return true;
        }

        /// <summary>
        /// Returns a summary string of the current state.
        /// </summary>
        public string ToString()
        {
            return $"SimulationState[Step={TimeStep}, Efficiency={Efficiency:F0}%, Time={ElapsedTime}s]";
        }
    }
}
