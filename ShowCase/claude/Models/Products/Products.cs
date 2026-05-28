using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FactorioModeler.Models.Products
{
    /// <summary>
    /// Represents a manufactured product in the Factorio production chain.
    /// Products are end-items or intermediate goods created by recipes.
    /// </summary>
    [Serializable]
    public class Product
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Factorio item ID reference for the product.
        /// </summary>
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable product name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the recipe ID that produces this product.
        /// </summary>
        [JsonPropertyName("recipeId")]
        public string RecipeId { get; set; }

        /// <summary>
        /// Gets or sets the product description and usage context.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon representation (emoji).
        /// </summary>
        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the output rate per machine speed tier.
        /// </summary>
        [JsonPropertyName("outputRate")]
        public double OutputRate { get; set; }

        /// <summary>
        /// Gets or sets the machine type used for production.
        /// </summary>
        [JsonPropertyName("machineType")]
        public string MachineType { get; set; }

        /// <summary>
        /// Gets or sets the product category for organization.
        /// </summary>
        [JsonPropertyName("category")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the emoji category indicator.
        /// </summary>
        [JsonPropertyName("categoryEmoji")]
        public string CategoryEmoji { get; set; }

        /// <summary>
        /// Gets or sets the speed tier multipliers for this product.
        /// </summary>
        [JsonPropertyName("speedTiers")]
        public Dictionary<string, double> SpeedTiers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is consumable.
        /// </summary>
        [JsonPropertyName("consumable")]
        public bool Consumable { get; set; }

        /// <summary>
        /// Gets or sets the quantity produced per crafting cycle.
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets whether the product is currently in production.
        /// </summary>
        [JsonIgnore]
        public bool IsInProduction { get; set; }

        /// <summary>
        /// Initializes a new instance of the Product class.
        /// </summary>
        public Product()
        {
            SpeedTiers = new Dictionary<string, double>();
        }

        /// <summary>
        /// Calculates effective throughput for a given machine speed.
        /// </summary>
        /// <param name="machineSpeed">The machine speed multiplier.</param>
        /// <returns>The effective output rate per second.</returns>
        public double CalculateEffectiveThroughput(double machineSpeed)
        {
            if (SpeedTiers.TryGetValue(machineSpeed.ToString(), out var tierSpeed))
            {
                return OutputRate * tierSpeed / CraftingTimeSeconds();
            }
            return OutputRate / CraftingTimeSeconds();
        }

        /// <summary>
        /// Gets the crafting time in seconds for this product.
        /// </summary>
        private int CraftingTimeSeconds()
        {
            if (!string.IsNullOrEmpty(RecipeId))
            {
                var recipe = ProductManager.Instance?.GetRecipeById(RecipeId);
                return recipe?.CraftingTimeSeconds ?? 1;
            }
            return 1;
        }

        /// <summary>
        /// Gets the machine configuration object by ID.
        /// </summary>
        /// <param name="machineId">The machine ID to retrieve.</param>
        /// <returns>The Machine or null if not found.</returns>
        public Machine GetMachineInfo(string machineId)
        {
            return ProductManager.Instance?.GetMachineById(machineType) ?? null;
        }

        /// <summary>
        /// Validates product configuration.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Id)) return false;
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (string.IsNullOrWhiteSpace(MachineType)) return false;
            return true;
        }

        /// <summary>
        /// Updates output rate based on new machine speed configuration.
        /// </summary>
        /// <param name="newSpeed">The new speed multiplier.</param>
        public void UpdateOutputRate(double newSpeed)
        {
            if (SpeedTiers == null)
            {
                SpeedTiers = new Dictionary<string, double>();
            }
            if (!SpeedTiers.ContainsKey(machineType))
            {
                SpeedTiers[machineType] = newSpeed;
            }
            else
            {
                SpeedTiers[machineType] = newSpeed;
            }
            OutputRate = CalculateEffectiveThroughput(newSpeed);
        }
    }

    /// <summary>
    /// Factory and accessor for Product instances.
    /// </summary>
    public class ProductManager
    {
        private static ProductManager _instance;
        private Dictionary<string, Product> _products;
        private Dictionary<string, Machine> _machines;
        private Dictionary<string, Recipe> _recipes;

        private ProductManager()
        {
            _products = new Dictionary<string, Product>();
            _machines = new Dictionary<string, Machine>();
            _recipes = new Dictionary<string, Recipe>();
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static ProductManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProductManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Registers a new product.
        /// </summary>
        /// <param name="product">The product to register.</param>
        public void RegisterProduct(Product product)
        {
            if (!string.IsNullOrEmpty(product.Id))
            {
                _products[product.Id] = product;
            }
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>The Product or null if not found.</returns>
        public Product GetProductById(string id)
        {
            return _products.TryGetValue(id, out var product) ? product : null;
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>Dictionary of all registered products.</returns>
        public Dictionary<string, Product> GetAllProducts()
        {
            return _products;
        }

        /// <summary>
        /// Registers a machine.
        /// </summary>
        /// <param name="machine">The machine to register.</param>
        public void RegisterMachine(Machine machine)
        {
            if (!string.IsNullOrEmpty(machine.Id))
            {
                _machines[machine.Id] = machine;
            }
        }

        /// <summary>
        /// Gets a machine by ID.
        /// </summary>
        /// <param name="id">The machine ID.</param>
        /// <returns>The Machine or null if not found.</returns>
        public Machine GetMachineById(string id)
        {
            return _machines.TryGetValue(id, out var machine) ? machine : null;
        }

        /// <summary>
        /// Registers a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to register.</param>
        public void RegisterRecipe(Recipe recipe)
        {
            if (!string.IsNullOrEmpty(recipe.Id))
            {
                _recipes[recipe.Id] = recipe;
            }
        }

        /// <summary>
        /// Gets a recipe by ID.
        /// </summary>
        /// <param name="id">The recipe ID.</param>
        /// <returns>The Recipe or null if not found.</returns>
        public Recipe GetRecipeById(string id)
        {
            return _recipes.TryGetValue(id, out var recipe) ? recipe : null;
        }

        /// <summary>
        /// Calculates throughput for a specific product using given machine.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="machineId">The machine ID.</param>
        /// <returns>The calculated throughput per minute.</returns>
        public double CalculateProductThroughput(string productId, string machineId)
        {
            var product = GetProductById(productId);
            var machine = GetMachineById(machineId);

            if (product == null) return 0;
            if (machine == null) return 0;

            double machineSpeed = machine.BaseSpeed;
            int craftingTime = product.CraftingTimeSeconds();

            return (product.Quantity / (double)craftingTime) * machineSpeed * 60;
        }
    }

    /// <summary>
    /// Represents categories for product organization.
    /// </summary>
    public static class ProductCategories
    {
        public const string Industrial = "Industrial";
        public const string Transport = "Transport";
        public const string Storage = "Storage";
        public const string Power = "Power";
        public const string Crafting = "Crafting";
        public const string RawMaterial = "RawMaterial";
    }

    /// <summary>
    /// Product configuration with category metadata.
    /// </summary>
    [Serializable]
    public class ProductCategoryInfo
    {
        /// <summary>
        /// Gets or sets the category display name.
        /// </summary>
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the category color code.
        /// </summary>
        [JsonPropertyName("color")]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets whether this is a sub-category.
        /// </summary>
        [JsonPropertyName("subCategory")]
        public bool SubCategory { get; set; }

        /// <summary>
        /// Gets the category color by category name.
        /// </summary>
        public static string GetCategoryColor(string categoryName)
        {
            return categoryName switch
            {
                ProductCategories.Industrial => "#e74c3c",
                ProductCategories.Transport => "#27ae60",
                ProductCategories.Storage => "#8e44ad",
                ProductCategories.Power => "#f39c12",
                ProductCategories.Crafting => "#3498db",
                ProductCategories.RawMaterial => "#1abc9c",
                _ => "#7f8c8d"
            };
        }
    }
}
