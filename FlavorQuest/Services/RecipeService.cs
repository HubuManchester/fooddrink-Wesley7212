using System.Text.Json;
using FlavorQuest.Models;

namespace FlavorQuest.Services;

/// <summary>
/// Provides recipe data management including loading, searching, filtering, and favorites.
/// All recipe data is loaded from an embedded JSON resource file.
/// </summary>
public class RecipeService
{
    private List<Recipe> _allRecipes = new();
    private List<Category> _categories = new();
    private HashSet<int> _favoriteIds = new();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes the service and loads recipe data from embedded resources.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await LoadRecipesAsync();
            await LoadFavoritesAsync();
            BuildCategories();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RecipeService initialization error: {ex.Message}");
            // Initialize with empty data on failure to prevent app crashes
            _allRecipes = new List<Recipe>();
            _categories = new List<Category>();
        }
    }

    /// <summary>
    /// Loads recipes from the embedded JSON resource file.
    /// </summary>
    private async Task LoadRecipesAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("recipes.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            _allRecipes = JsonSerializer.Deserialize<List<Recipe>>(json, _jsonOptions) ?? new List<Recipe>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading recipes: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads saved favorite recipe IDs from preferences.
    /// </summary>
    private Task LoadFavoritesAsync()
    {
        try
        {
            var json = Preferences.Get(Helpers.Constants.FavoritesKey, "[]");
            _favoriteIds = JsonSerializer.Deserialize<HashSet<int>>(json) ?? new HashSet<int>();
        }
        catch
        {
            _favoriteIds = new HashSet<int>();
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves favorite recipe IDs to preferences.
    /// </summary>
    private void SaveFavorites()
    {
        try
        {
            var json = JsonSerializer.Serialize(_favoriteIds);
            Preferences.Set(Helpers.Constants.FavoritesKey, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving favorites: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds category list from the loaded recipes.
    /// </summary>
    private void BuildCategories()
    {
        var categoryColors = new Dictionary<string, Color>
        {
            { "Breakfast", Color.FromArgb("#FF6B35") },
            { "Lunch", Color.FromArgb("#4CAF50") },
            { "Dinner", Color.FromArgb("#2196F3") },
            { "Dessert", Color.FromArgb("#E91E63") },
            { "Snack", Color.FromArgb("#FF9800") },
            { "Beverage", Color.FromArgb("#9C27B0") },
            { "Salad", Color.FromArgb("#8BC34A") },
            { "Soup", Color.FromArgb("#795548") },
            { "Pasta", Color.FromArgb("#F44336") },
            { "Seafood", Color.FromArgb("#00BCD4") }
        };

        var uniqueCategories = _allRecipes
            .Select(r => r.Category)
            .Distinct()
            .OrderBy(c => c);

        _categories = uniqueCategories.Select(c => new Category
        {
            Name = c,
            Description = $"Discover delicious {c.ToLower()} recipes",
            Icon = GetCategoryIcon(c),
            Color = categoryColors.GetValueOrDefault(c, Colors.Gray)
        }).ToList();
    }

    /// <summary>
    /// Returns an emoji icon for the given category name.
    /// </summary>
    private static string GetCategoryIcon(string category)
    {
        return category.ToLower() switch
        {
            "breakfast" => "🥞",
            "lunch" => "🥪",
            "dinner" => "🍽️",
            "dessert" => "🍰",
            "snack" => "🍿",
            "beverage" => "🥤",
            "salad" => "🥗",
            "soup" => "🍜",
            "pasta" => "🍝",
            "seafood" => "🦞",
            _ => "🍳"
        };
    }

    /// <summary>
    /// Gets all available recipe categories.
    /// </summary>
    public List<Category> GetCategories() => _categories;

    /// <summary>
    /// Gets all recipes.
    /// </summary>
    public List<Recipe> GetAllRecipes() => ApplyFavoriteStatus(_allRecipes);

    /// <summary>
    /// Gets recipes filtered by category.
    /// </summary>
    public List<Recipe> GetRecipesByCategory(string category)
    {
        var recipes = _allRecipes
            .Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return ApplyFavoriteStatus(recipes);
    }

    /// <summary>
    /// Searches recipes by name, description, ingredients, or tags.
    /// </summary>
    public List<Recipe> SearchRecipes(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllRecipes();

        var lowerQuery = query.ToLower().Trim();
        var results = _allRecipes
            .Where(r =>
                r.Name.ToLower().Contains(lowerQuery) ||
                r.Description.ToLower().Contains(lowerQuery) ||
                r.Category.ToLower().Contains(lowerQuery) ||
                r.Ingredients.Any(i => i.ToLower().Contains(lowerQuery)) ||
                r.Tags.Any(t => t.ToLower().Contains(lowerQuery)))
            .ToList();

        return ApplyFavoriteStatus(results);
    }

    /// <summary>
    /// Gets a specific recipe by ID. Returns null if not found.
    /// </summary>
    public Recipe? GetRecipeById(int id)
    {
        var recipe = _allRecipes.FirstOrDefault(r => r.Id == id);
        if (recipe != null)
        {
            recipe.IsFavorite = _favoriteIds.Contains(recipe.Id);
        }
        return recipe;
    }

    /// <summary>
    /// Gets a random recipe, optionally from a specific category.
    /// </summary>
    public Recipe? GetRandomRecipe(string? category = null)
    {
        var pool = string.IsNullOrWhiteSpace(category)
            ? _allRecipes
            : _allRecipes.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        if (pool.Count == 0)
            return null;

        var random = System.Security.Cryptography.RandomNumberGenerator.GetInt32(pool.Count);
        return ApplyFavoriteStatus(new List<Recipe> { pool[random] }).First();
    }

    /// <summary>
    /// Toggles the favorite status of a recipe and persists the change.
    /// </summary>
    public void ToggleFavorite(int recipeId)
    {
        if (_favoriteIds.Contains(recipeId))
            _favoriteIds.Remove(recipeId);
        else
            _favoriteIds.Add(recipeId);

        SaveFavorites();
    }

    /// <summary>
    /// Checks whether a recipe is favorited.
    /// </summary>
    public bool IsFavorite(int recipeId) => _favoriteIds.Contains(recipeId);

    /// <summary>
    /// Gets all favorited recipes.
    /// </summary>
    public List<Recipe> GetFavoriteRecipes()
    {
        var favorites = _allRecipes
            .Where(r => _favoriteIds.Contains(r.Id))
            .ToList();
        return ApplyFavoriteStatus(favorites);
    }

    /// <summary>
    /// Gets suggested recipes based on tags (excluding the given recipe).
    /// </summary>
    public List<Recipe> GetSuggestedRecipes(int excludeRecipeId, int count = 4)
    {
        var currentRecipe = _allRecipes.FirstOrDefault(r => r.Id == excludeRecipeId);
        if (currentRecipe == null)
            return new List<Recipe>();

        var suggestions = _allRecipes
            .Where(r => r.Id != excludeRecipeId && r.Tags.Any(t => currentRecipe.Tags.Contains(t)))
            .Take(count)
            .ToList();

        return ApplyFavoriteStatus(suggestions);
    }

    /// <summary>
    /// Applies favorite status to a list of recipes.
    /// </summary>
    private List<Recipe> ApplyFavoriteStatus(List<Recipe> recipes)
    {
        foreach (var recipe in recipes)
        {
            recipe.IsFavorite = _favoriteIds.Contains(recipe.Id);
        }
        return recipes;
    }
}
