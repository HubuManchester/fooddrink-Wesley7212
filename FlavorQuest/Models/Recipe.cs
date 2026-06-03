using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FlavorQuest.Models;

/// <summary>
/// Represents a recipe with its ingredients, instructions, and metadata.
/// </summary>
public partial class Recipe : ObservableObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("prepTimeMinutes")]
    public int PrepTimeMinutes { get; set; }

    [JsonPropertyName("cookTimeMinutes")]
    public int CookTimeMinutes { get; set; }

    [JsonPropertyName("servings")]
    public int Servings { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "Easy";

    [JsonPropertyName("ingredients")]
    public List<string> Ingredients { get; set; } = new();

    [JsonPropertyName("instructions")]
    public List<string> Instructions { get; set; } = new();

    [JsonPropertyName("nutritionPerServing")]
    public NutritionInfo? Nutrition { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Indicates whether this recipe has been saved as a favorite by the user.
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    private bool isFavorite;

    /// <summary>
    /// Formatted total time string for display.
    /// </summary>
    [JsonIgnore]
    public string TotalTimeDisplay => $"{PrepTimeMinutes + CookTimeMinutes} min";

    /// <summary>
    /// Formatted difficulty badge color for display.
    /// </summary>
    [JsonIgnore]
    public Color DifficultyColor => Difficulty switch
    {
        "Easy" => Colors.Green,
        "Medium" => Colors.Orange,
        "Hard" => Colors.Red,
        _ => Colors.Gray
    };

    public override string ToString() => Name;
}

/// <summary>
/// Nutritional information per serving for a recipe.
/// </summary>
public class NutritionInfo
{
    [JsonPropertyName("calories")]
    public int Calories { get; set; }

    [JsonPropertyName("proteinGrams")]
    public double ProteinGrams { get; set; }

    [JsonPropertyName("carbsGrams")]
    public double CarbsGrams { get; set; }

    [JsonPropertyName("fatGrams")]
    public double FatGrams { get; set; }
}
