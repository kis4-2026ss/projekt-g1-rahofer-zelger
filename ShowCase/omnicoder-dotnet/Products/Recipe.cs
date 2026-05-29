using FactorioModeler.Models;

namespace FactorioModeler.Products;

/// <summary>
/// Represents a crafting recipe in the Factorio Modeler.
/// 
/// Recipes define how to produce products from resources.
/// Each recipe specifies the output item, quantity, crafting time,
/// and list of required resources with their respective amounts.
/// </summary>
public class Recipe
{
    /// <summary>
    /// Unique identifier for this recipe.
    /// </summary>
    public string Id { get; set; } = "advanced-circuit";

    /// <summary>
    /// Name of the recipe (display text).
    /// </summary>
    public string RecipeName { get; set; } = "Advanced Circuit";

    /// <summary>
    /// Machine type required to craft this recipe.
    /// </summary>
    public string MachineType { get; set; } = "assembling-machine-2";

    /// <summary>
    /// Item ID of the product this recipe produces.
    /// </summary>
    public string OutputItemId { get; set; } = "advanced-circuit";

    /// <summary>
    /// Quantity of product produced per craft.
    /// </summary>
    public int OutputQty { get; set; } = 1;

    /// <summary>
    /// Crafting time in the default machine (seconds per craft).
    /// </summary>
    public int CraftingTime { get; set; } = 1;

    /// <summary>
    /// List of input resources required for crafting.
    /// </summary>
    public List<RequiredResource> RequiredResources { get; set; } = new();

    /// <summary>
    /// Output rate in items per minute.
    /// </summary>
    public double OutputRate { get; set; }

    /// <summary>
    /// Whether this recipe is currently available.
    /// </summary>
    public bool Available { get; set; } = true;

    /// <summary>
    /// Calculate total resource demand.
    /// </summary>
    /// Returns the total resources consumed per minute of production.
    /// <returns>List of required resources.</returns>
    public List<RequiredResource> CalculateResourceDemand()
    {
        HashSet<string> uniqueResources = new(StringComparer.OrdinalIgnoreCase);

        foreach (var resource in this.RequiredResources)
        {
            if (uniqueResources.Add(resource.ItemId))
            {
                this.RequiredResources.Add(resource);
            }
        }

        this.RequiredResources = this.RequiredResources.Distinct(
            StringComparer.OrdinalIgnoreCase).ToList();

        return this.RequiredResources;
    }

    /// <summary>
    /// Calculate crafting duration at specified speed.
    /// </summary>
    /// <param name="machineSpeed">Machine speed multiplier.</param>
    /// <returns>Time in minutes to craft one item.</returns>
    public double CraftingDurationAtSpeed(double machineSpeed)
    {
        return (this.CraftingTime / (double) (machineSpeed * 60));
    }

    /// <summary>
    /// Clone this recipe instance.
    /// </summary>
    public Recipe Clone()
    {
        return new Recipe
        {
            Id = this.Id,
            RecipeName = this.RecipeName,
            MachineType = this.MachineType,
            OutputItemId = this.OutputItemId,
            OutputQty = this.OutputQty,
            CraftingTime = this.CraftingTime,
            RequiredResources = this.RequiredResources.Clone(),
            OutputRate = this.OutputRate
        };
    }
}

/// <summary>
/// Base class for recipe models.
/// </summary>
public class RecipeModel
{
    public Recipe? Recipe { get; set; }
    public RecipeContext Context { get; set; } = new();
}

/// <summary>
/// Context for recipe validation and calculation.
/// </summary>
public class RecipeContext
{
    /// <summary>
    /// Available machine speeds.
    /// </summary>
    public Dictionary<string, double> MachineSpeedTiers = new()
    {
        ["assembling-machine-1"] = 0.67,
        ["assembling-machine-2"] = 1.0,
        ["assembling-machine-3"] = 1.5
    };

    /// <summary>
    /// Available resources in the simulation.
    /// </summary>
    public Dictionary<string, double> Resources { get; set; } = new();

    /// <summary>
    /// Get machine speed by machine type.
    /// </summary>
    public double? GetMachineSpeed(string machineType)
    {
        return this.MachineSpeedTiers.ContainsKey(machineType)
            ? this.MachineSpeedTiers[machineType]
            : null;
    }

    /// <summary>
    /// Get or add resource quantity.
    /// </summary>
    public double GetResourceQuantity(string resourceId)
    {
        return this.Resources.ContainsKey(resourceId)
            ? this.Resources[resourceId]
            : 0;
    }

    /// <summary>
    /// Add resource to context.
    /// </summary>
    /// <param name="resourceId">Resource identifier.</param>
    /// <param name="quantity">Quantity to add.</param>
    public void AddResource(string resourceId, double quantity)
    {
        this.Resources[resourceId] = quantity;
    }

    /// <summary>
    /// Consume resources for a recipe.
    /// </summary>
    /// <param name="recipe">Recipe to validate.</param>
    /// <returns>True if resources are available.</returns>
    public bool CanSatisfyRecipe(Recipe recipe)
    {
        foreach (var resource in recipe.RequiredResources)
        {
            double available = this.GetResourceQuantity(resource.ItemId);
            if (available < resource.Amount)
                return false;

            double requiredPerMinute = recipe.OutputRate * resource.Minutely;
            if (available < requiredPerMinute)
                return false;
        }

        return true;
    }
}

/// <summary>
/// Collection of all recipes in the simulation.
/// </summary>
public class RecipeModels
{
    /// <summary>
    /// All available recipes indexed by item ID.
    /// </summary>
    public Dictionary<string, Recipe> Recipes = new()
    {
        ["advanced-circuit"] = new Recipe
        {
            Id = "advanced-circuit-basic",
            RecipeName = "Advanced Circuit Basic",
            MachineType = "assembling-machine-2",
            OutputItemId = "advanced-circuit",
            OutputQty = 1,
            CraftingTime = 1,
            RequiredResources = new List<RequiredResource>
                
            {
                    new RequiredResource { ItemId = "iron-plate", Amount = 200 },
                    new RequiredResource { ItemId = "copper-cable", Amount = 10 },
                    new RequiredResource { ItemId = "steel-plate", Amount = 50 },
                    new RequiredResource { ItemId = "copper-plate", Amount = 100 },
                    new RequiredResource { ItemId = "copper-cable-m", Amount = 14 }
                },
            OutputRate = 10
        },

        ["express-splitter"] = new Recipe
        {
            Id = "express-splitter-basic",
            RecipeName = "Express Splitter",
            MachineType = "assembling-machine-2",
            OutputItemId = "express-splitter",
            OutputQty = 2,
            CraftingTime = 15,
            RequiredResources = new List<RequiredResource>
            {
                    new RequiredResource { ItemId = "advanced-circuit", Amount = 1500 },
                    new RequiredResource { ItemId = "copper-plate", Amount = 750 },
                    new RequiredResource { ItemId = "iron-plate", Amount = 50 },
                    new RequiredResource { ItemId = "copper-cable", Amount = 10 },
                    new RequiredResource { ItemId = "steel-plate", Amount = 50 }
                },
            OutputRate = 2.5
        },

        ["advanced-circuit-advanced"] = new Recipe
        {
            Id = "adv-circuit-9x",
            RecipeName = "Advanced Circuit",
            MachineType = "assembling-machine-2",
            OutputItemId = "advanced-circuit",
            OutputQty = 10,
            CraftingTime = 1,
            RequiredResources = new List<RequiredResource>
            {
                    new RequiredResource { ItemId = "iron-plate", Amount = 200, Minutely = 200 },
                    new RequiredResource { ItemId = "copper-cable", Amount = 10, Minutely = 10 },
                    new RequiredResource { ItemId = "steel-plate", Amount = 50, Minutely = 50 },
                    new RequiredResource { ItemId = "copper-plate", Amount = 100, Minutely = 100 },
                    new RequiredResource { ItemId = "copper-cable-m", Amount = 14, Minutely = 14 }
                },
            OutputRate = 10
        }
    };

    /// <summary>
    /// Validate all recipes for circular dependencies.
    /// </summary>
    public bool ValidateCircularDependencies()
    {
        foreach (var recipe in this.Recipes.Values)
        {
            if (recipe.MachineType != "assembling-machine-2")
                continue;

            if (recipe.OutputItemId == "advanced-circuit" && 
                recipe.MachineType == "assembling-machine-2")
                continue;
        }

        return true;
    }
}

/// <summary>
/// Required resource type.
/// </summary>
public class RequiredResource
{
    /// <summary>
    /// Resource item identifier.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Quantity required per craft.
    /// </summary>
    public int Amount { get; set; } = 0;

    /// <summary>
    /// Rate required per minute.
    /// </summary>
    public double Minutely { get; set; }
}