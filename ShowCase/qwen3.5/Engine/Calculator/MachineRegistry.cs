using System.Collections.Generic;

namespace Factorio.Core.Engine
{
    internal static class MachineRegistry
    {
        public static ICollection<Factorio.Core.Engine.Models.Machine> Machines => machines.AsReadOnly();

        private static readonly List<Factorio.Core.Engine.Models.Machine> machines = new()
        {
            new Factorio.Core.Engine.Models.Machine
            {
                Id = "assembling-machine-2",
                Name = "Assembling Machine",
                Type = "assembling-machine-2",
                Emoji = "🏭",
                MinSpeed = 0,
                MaxSpeed = 1,
                Category = "industrial",
                MaxOutputs = 10,
                CategoryColor = "red",
                CategoryName = "Machine",
                SpeedMultiplier = 1.0
            },
            new Factorio.Core.Engine.Models.Machine
            {
                Id = "assembling-machine-3",
                Name = "Assembling Machine Level 3",
                Type = "assembling-machine-3",
                Emoji = "🏭",
                MinSpeed = 0,
                MaxSpeed = 1,
                Category = "industrial",
                MaxOutputs = 10,
                CategoryColor = "red",
                CategoryName = "Machine",
                SpeedMultiplier = 1.5
            },
            new Factorio.Core.Engine.Models.Machine
            {
                Id = "furnace",
                Name = "Furnace",
                Type = "furnace",
                Emoji = "🔥",
                MinSpeed = 0,
                MaxSpeed = 1,
                Category = "industrial",
                MaxOutputs = 10,
                CategoryColor = "red",
                CategoryName = "Machine",
                SpeedMultiplier = 1.0
            },
            new Factorio.Core.Engine.Models.Machine
            {
                Id = "belt",
                Name = "Belt",
                Type = "conveyor-belt",
                Emoji = "🟦",
                MinSpeed = 0,
                MaxSpeed = 1,
                Category = "belt",
                MaxOutputs = 1,
                CategoryColor = "green",
                CategoryName = "Belt",
                SpeedMultiplier = 1.0
            },
            new Factorio.Core.Engine.Models.Machine
            {
                Id = "buffer",
                Name = "Buffer",
                Type = "buffer",
                Emoji = "🟦",
                MinSpeed = 0,
                MaxSpeed = 1,
                Category = "storage",
                MaxOutputs = 1,
                CategoryColor = "purple",
                CategoryName = "Storage",
                SpeedMultiplier = 1.0
            }
        };
    }
}