namespace FactorioModeler.Engine.Models
{
    /// <summary>
    /// Machine registry with speed multipliers per Technical Architecture specs
    /// Lv1: 0.67, Lv2: 1.0, Lv3: 1.5
    /// </summary>
    public class Machine
    {
        /// <summary>
        /// Machine level enumeration for speed tier lookup
        /// </summary>
        public enum Level { Lvl1, Lvl2, Lvl3 }

        /// <summary>
        /// Machine speed multipliers per Gherkin Test 1 specs
        /// </summary>
        public static readonly Dictionary<int, double> SpeedMultipliers = new()
        {
            { 1, 0.67 }, // Assembling Machine Lvl 1
            { 2, 1.0 },  // Assembling Machine Lvl 2
            { 3, 1.5 }   // Assembling Machine Lvl 3
        };

        /// <summary>
        /// Get speed multiplier for given level
        /// </summary>
        public static double GetMultiplier(int level)
        {
            if (!SpeedMultipliers.TryGetValue(level, out double speed))
                throw new ArgumentException($"Invalid machine level: {level}", nameof(level));
            return speed;
        }

        /// <summary>
        /// Get by level (matches Technical Architecture)
        /// </summary>
        public static Machine GetByLevel(int level)
        {
            return new Machine
            {
                Level = level,
                Speed = GetMultiplier(level),
                Name = level switch
                {
                    1 => "Assembling Machine Lvl 1",
                    2 => "Assembling Machine Lvl 2",
                    3 => "Assembling Machine Lvl 3",
                    _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
                }
            };
        }

        public int Level { get; }
        public double Speed { get; }
        public string Name { get; }
    }
}