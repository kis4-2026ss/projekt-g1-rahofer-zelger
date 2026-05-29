// ThroughputConstants.cs
// Factorio Modeler Engine Core - Constants and Calculation Parameters
// Revision 4.0.0

namespace Factorio.Engine.Calculator
{
    /// <summary>
    /// Throughput calculation constants for the Factorio Modeler engine.
    /// Contains machine speed multipliers and precision tolerances.
    /// </summary>
    public static class ThroughputConstants
    {
        /// <summary>
        /// Assembling Machine Lvl 1 speed multiplier
        /// </summary>
        public const double ASM_Level_1_Speed = 0.67;

        /// <summary>
        /// Assembling Machine Lvl 2-4 speed multiplier
        /// </summary>
        public const double ASM_Level_2_4_Speed = 1.0;

        /// <summary>
        /// Assembling Machine Lvl 5 speed multiplier
        /// </summary>
        public const double ASM_Level_5_Speed = 1.5;

        /// <summary>
        /// Precision tolerance per minute (±0.001)
        /// </summary>
        public const double Tolerance = 0.001;

        /// <summary>
        /// Minutes per hour
        /// </summary>
        public const int MinutesPerHour = 60;
    }
}
