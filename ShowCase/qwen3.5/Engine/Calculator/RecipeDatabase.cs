// RecipeDatabase.cs
// Factorio Modeler Engine Core - Recipe Database Management
// Revision 4.0.0

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace Factorio.Engine.Calculator
{
    /// <summary>
    /// Recipe database management for Factorio recipe loading and persistence.
    /// </summary>
    public static class RecipeDatabase
    {
        private static readonly string DefaultPath = "factorio_recipes_and_machines.json";
        private static string? _currentPath;

        /// <summary>
        /// Loads recipes from JSON file.
        /// </summary>
        /// <param name="filePath">Path to recipes JSON file</param>
        /// <returns>List of loaded recipes</returns>
        public static List<Recipe> LoadRecipes(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Recipe file not found: {filePath}");

            var jsonContent = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var recipes = JsonSerializer.Deserialize<List<Recipe>>(jsonContent, options);
            return recipes ?? new List<Recipe>();
        }

        /// <summary>
        /// Loads machines registry from JSON file.
        /// </summary>
        /// <param name="filePath">Path to machines JSON file</param>
        /// <returns>List of loaded machines</returns>
        public static List<Machine> LoadMachines(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Machines file not found: {filePath}");

            var jsonContent = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var recipeData = JsonSerializer.Deserialize<RecipeData>(jsonContent, options);
            return recipeData?.Machines ?? new List<Machine>();
        }

        /// <summary>
        /// Saves recipes to JSON file.
        /// </summary>
        public static void SaveRecipes(string filePath, List<Recipe> recipes)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var data = new RecipeData
            {
                Recipes = recipes,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            File.WriteAllText(filePath, JsonSerializer.Serialize(data, options));
        }

        /// <summary>
        /// Gets the currently configured recipe file path.
        /// </summary>
        public static string GetCurrentRecipePath()
        {
            return _currentPath ?? DefaultPath;
        }

        /// <summary>
        /// Sets the recipe file path for the current session.
        /// </summary>
        public static void SetRecipePath(string path)
        {
            if (!File.Exists(path))
            {
                _currentPath = null;
                return;
            }

            _currentPath = path;
        }

        /// <summary>
        /// Recipe data container class.
        /// </summary>
        private class RecipeData
        {
            public List<Recipe> Recipes { get; set; }
            public List<Machine> Machines { get; set; }
            public string Version { get; set; }
            public string Timestamp { get; set; }
        }
    }

    /// <summary>
    /// Machine registry for Factorio machine types.
    /// </summary>
    public class Machine
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Emoji { get; set; }
        public bool SpeedMultiplier { get; set; }
        public int MinSpeed { get; set; }
        public int MaxSpeed { get; set; }
        public int MaxOutputs { get; set; }
        public string Category { get; set; }
        public string CategoryName { get; set; }
        public double? SpeedMultiplier { get; set; }
    }
}
