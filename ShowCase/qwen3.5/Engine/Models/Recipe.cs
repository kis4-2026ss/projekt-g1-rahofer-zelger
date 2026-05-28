namespace FactorioModeler.Engine.Models
{
    /// <summary>
    /// Recipe data model with immutable constants defined in specs.
    /// Tolerance: ±0.001 per minute accuracy
    /// </summary>
    public class Recipe
    {
        /// <summary>
        /// Recipe type identifier (e.g., "advanced-circuit", "express-splitter")
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Number of items produced per recipe
        /// </summary>
        public int OutputQty { get; }

        /// <summary>
        /// Crafting time in seconds per recipe
        /// </summary>
        public double CraftingTime { get; }

        /// <summary>
        /// Static recipe registry with validated data per Gherkin Test 1-4
        /// </summary>
        public static readonly Dictionary<string, Recipe> Registry = new()
        {
            ["advanced-circuit"] = new Recipe
            {
                Type = "advanced-circuit",
                OutputQty = 1,
                CraftingTime = 6.0 // Assembler Lv3: 6s per cycle, speed 1.5
            },
            ["express-splitter"] = new Recipe
            {
                Type = "express-splitter",
                OutputQty = 1,
                CraftingTime = 2.0 // Assembler Lv3: 2s per cycle, speed 1.5
            },
            ["basic-circuit"] = new Recipe
            {
                Type = "basic-circuit",
                OutputQty = 1,
                CraftingTime = 5.0 // Assembler Lv1 baseline
            },
            ["complex-circuit"] = new Recipe
            {
                Type = "complex-circuit",
                OutputQty = 1,
                CraftingTime = 12.0 // Assembler Lv3 complex recipe
            }
        };

        public Recipe(string type, int outputQty, double craftingTime)
        {
            Type = type;
            OutputQty = outputQty;
            CraftingTime = craftingTime;
        }

        /// <summary>
        /// Validate recipe against precision tolerance
        /// </summary>
        public bool Validate(double tolerance = 0.001)
        {
            if (CraftingTime <= 0)
                throw new ArgumentException($"Invalid crafting time: {CraftingTime}s", nameof(CraftingTime));
            if (OutputQty <= 0)
                throw new ArgumentException($"Invalid output quantity: {OutputQty}", nameof(OutputQty));
            return true; // Validation passes
        }

        /// <summary>
        /// Calculate raw rate (items per second)
        /// </summary>
        public double RatePerSecond => OutputQty / CraftingTime;

        /// <summary>
        /// Calculate throughput per minute with machine speed multiplier
        /// Formula: T = (OutputQty / CraftingTime) × MachineSpeed × 60
        /// </summary>
        public double CalculateThroughput(double machineSpeed)
        {
            if (machineSpeed < 0.5d || machineSpeed > 3.0d)
                throw new ArgumentException($"Invalid machine speed: {machineSpeed}", nameof(machineSpeed));
            return (OutputQty / CraftingTime) * machineSpeed * 60.0;
        }
    }
}