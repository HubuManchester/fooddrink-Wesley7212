using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the recipe detail page. Shows full recipe information
/// including ingredients, instructions, nutrition, and related recipes.
/// Supports Text-to-Speech for accessibility.
/// </summary>
[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly TextToSpeechService _ttsService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private int recipeId;

    [ObservableProperty]
    private Recipe? recipe;

    [ObservableProperty]
    private ObservableCollection<Recipe> suggestions = new();

    [ObservableProperty]
    private bool isTtsSpeaking;

    public RecipeDetailViewModel(
        RecipeService recipeService,
        TextToSpeechService ttsService,
        SettingsService settingsService)
    {
        _recipeService = recipeService;
        _ttsService = ttsService;
        _settingsService = settingsService;
        Title = "Recipe";
    }

    /// <summary>
    /// Called when the RecipeId query parameter is set. Loads the recipe.
    /// </summary>
    partial void OnRecipeIdChanged(int value)
    {
        LoadRecipe(value);
    }

    /// <summary>
    /// Loads the recipe by ID with error handling.
    /// </summary>
    private void LoadRecipe(int id)
    {
        try
        {
            IsBusy = true;

            Recipe = _recipeService.GetRecipeById(id);
            if (Recipe != null)
            {
                Title = Recipe.Name;

                // Load suggested recipes
                var suggested = _recipeService.GetSuggestedRecipes(id);
                Suggestions = new ObservableCollection<Recipe>(suggested);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadRecipe error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Toggles the favorite status of the current recipe.
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Recipe == null)
            return;

        try
        {
            _recipeService.ToggleFavorite(Recipe.Id);
            Recipe.IsFavorite = !Recipe.IsFavorite;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleFavorite error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads the recipe ingredients aloud using Text-to-Speech.
    /// Uses TextToSpeech hardware feature for accessibility.
    /// </summary>
    [RelayCommand]
    private async Task ReadIngredientsAsync()
    {
        if (Recipe?.Ingredients == null || !_settingsService.CurrentSettings.TextToSpeechEnabled)
        {
            await Shell.Current.DisplayAlert("Text-to-Speech", "TTS is disabled in settings.", "OK");
            return;
        }

        try
        {
            IsTtsSpeaking = true;
            await _ttsService.SpeakIngredientsAsync(Recipe.Ingredients);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TTS ingredients error: {ex.Message}");
        }
        finally
        {
            IsTtsSpeaking = false;
        }
    }

    /// <summary>
    /// Reads the recipe instructions aloud using Text-to-Speech.
    /// Uses TextToSpeech hardware feature for accessibility.
    /// </summary>
    [RelayCommand]
    private async Task ReadInstructionsAsync()
    {
        if (Recipe?.Instructions == null || !_settingsService.CurrentSettings.TextToSpeechEnabled)
        {
            await Shell.Current.DisplayAlert("Text-to-Speech", "TTS is disabled in settings.", "OK");
            return;
        }

        try
        {
            IsTtsSpeaking = true;
            await _ttsService.SpeakInstructionsAsync(Recipe.Instructions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TTS instructions error: {ex.Message}");
        }
        finally
        {
            IsTtsSpeaking = false;
        }
    }

    /// <summary>
    /// Stops any ongoing text-to-speech.
    /// </summary>
    [RelayCommand]
    private void StopSpeaking()
    {
        _ttsService.Stop();
        IsTtsSpeaking = false;
    }

    /// <summary>
    /// Navigates to a suggested recipe.
    /// </summary>
    [RelayCommand]
    private async Task GoToSuggestionAsync(Recipe? suggestion)
    {
        if (suggestion == null)
            return;

        await _settingsService.TriggerHapticFeedbackAsync();

        try
        {
            await Shell.Current.GoToAsync($"recipeDetail?recipeId={suggestion.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to the recipe list for the recipe's category.
    /// </summary>
    [RelayCommand]
    private async Task GoToCategoryAsync()
    {
        if (Recipe == null)
            return;

        try
        {
            await Shell.Current.GoToAsync($"recipeList?category={Uri.EscapeDataString(Recipe.Category)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }
}
