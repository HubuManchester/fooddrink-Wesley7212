using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the recipe list page. Handles category filtering,
/// search results, and recipe selection.
/// </summary>
[QueryProperty(nameof(Category), "category")]
[QueryProperty(nameof(Search), "search")]
public partial class RecipeListViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<Recipe> recipes = new();

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string search = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isEmpty;

    public RecipeListViewModel(RecipeService recipeService, SettingsService settingsService)
    {
        _recipeService = recipeService;
        _settingsService = settingsService;
        Title = "Recipes";
    }

    /// <summary>
    /// Called when the Category query parameter is set. Loads filtered recipes.
    /// </summary>
    partial void OnCategoryChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Title = Uri.UnescapeDataString(value);
            LoadRecipes();
        }
    }

    /// <summary>
    /// Called when the Search query parameter is set. Performs recipe search.
    /// </summary>
    partial void OnSearchChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Title = $"Search: {Uri.UnescapeDataString(value)}";
            LoadSearchResults();
        }
    }

    /// <summary>
    /// Loads recipes for the current category.
    /// </summary>
    private void LoadRecipes()
    {
        try
        {
            IsBusy = true;
            var results = _recipeService.GetRecipesByCategory(Category);
            Recipes = new ObservableCollection<Recipe>(results);
            IsEmpty = Recipes.Count == 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadRecipes error: {ex.Message}");
            IsEmpty = true;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Performs a search and loads matching recipes.
    /// </summary>
    private void LoadSearchResults()
    {
        try
        {
            IsBusy = true;
            var decodedSearch = Uri.UnescapeDataString(Search);
            var results = _recipeService.SearchRecipes(decodedSearch);
            Recipes = new ObservableCollection<Recipe>(results);
            IsEmpty = Recipes.Count == 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            IsEmpty = true;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Refreshes the recipe list (for pull-to-refresh).
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        IsRefreshing = true;
        if (!string.IsNullOrEmpty(Search))
            LoadSearchResults();
        else
            LoadRecipes();
    }

    /// <summary>
    /// Navigates to the recipe detail page.
    /// </summary>
    [RelayCommand]
    private async Task GoToRecipeAsync(Recipe? recipe)
    {
        if (recipe == null)
            return;

        await _settingsService.TriggerHapticFeedbackAsync();

        try
        {
            await Shell.Current.GoToAsync($"recipeDetail?recipeId={recipe.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles the favorite status of a recipe from the list.
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite(Recipe? recipe)
    {
        if (recipe == null)
            return;

        try
        {
            _recipeService.ToggleFavorite(recipe.Id);
            recipe.IsFavorite = !recipe.IsFavorite;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleFavorite error: {ex.Message}");
        }
    }
}
