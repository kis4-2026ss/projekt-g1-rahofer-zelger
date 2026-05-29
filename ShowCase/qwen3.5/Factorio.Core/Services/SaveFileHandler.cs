using System;
using System.Collections.Generic;
using System.IO;

namespace Factorio.Core.Services
{
    /// <summary>
    /// Service for loading and managing factory save files.
    /// </summary>
    public class SaveFileHandler
    {
        private readonly string _dataPath;

        /// <summary>
        /// Initializes a new instance with the data path.
        /// </summary>
        /// <param name="dataPath">Path to data directory.</param>
        public SaveFileHandler(string dataPath)
        {
            _dataPath = dataPath;
        }

        /// <summary>
        /// Loads recipes from JSON file.
        /// </summary>
        /// <param name="recipePath">Path to recipes JSON.</param>
        /// <param name="machines">Machine definitions.</param>
        /// <returns>Loaded recipes.</returns>
        public IReadOnlyList<Recipe> InitializeRecipes(string recipePath, IList<Machine> machines)
        {
            if (!File.Exists(recipePath))
            {
                throw new FileNotFoundException($"Recipe file not found: {recipePath}");
            }

            var recipesJson = File.ReadAllText(recipePath);
            var recipes = System.Text.Json.JsonSerializer.Deserialize<List<Recipe>>(recipesJson);

            foreach (var recipe in recipes)
            {
                recipe.MachineType = machines.Find(m => m.Id == recipe.MachineType)?.Type ?? "assembling-machine-2";
            }

            return recipes;
        }

        /// <summary>
        /// Applies speed multiplier to recipe.
        /// </summary>
        /// <param name="recipe">Recipe to modify.</param>
        /// <param name="multiplier">Speed multiplier (0.0-1.0).</param>
        public void ApplySpeedMultiplier(Recipe recipe, double multiplier)
        {
            if (multiplier < 0 || multiplier > 1)
                throw new ArgumentOutOfRangeException(nameof(multiplier), "Must be between 0.0 and 1.0");

            recipe.MachineSpeed = multiplier;
            recipe.OutputRate *= multiplier;
        }

        /// <summary>
        /// Gets effective crafting time from speed multiplier.
        /// </summary>
        public int GetEffectiveCraftingTime(int originalCraftingTime, double speedMultiplier)
        {
            if (speedMultiplier == 0) return originalCraftingTime;
            return (int)Math.Ceiling(originalCraftingTime / speedMultiplier);
        }
    }
}