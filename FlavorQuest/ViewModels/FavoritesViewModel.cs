using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the favorites page. Displays saved recipes with
/// the ability to remove favorites and navigate to recipe details.
/// </summary>
public partial class FavoritesViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<Recipe> favoriteRecipes = new();

    [ObservableProperty]
    private bool isEmpty = true;

    [ObservableProperty]
    private bool isRefreshing;

    public FavoritesViewModel(RecipeService recipeService, SettingsService settingsService)
    {
        _recipeService = recipeService;
        _settingsService = settingsService;
        Title = "Favorites";
    }

    /// <summary>
    /// Loads favorite recipes when the page appears.
    /// </summary>
    [RelayCommand]
    private void LoadFavorites()
    {
        try
        {
            IsBusy = true;
            var favorites = _recipeService.GetFavoriteRecipes();
            FavoriteRecipes = new ObservableCollection<Recipe>(favorites);
            IsEmpty = FavoriteRecipes.Count == 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadFavorites error: {ex.Message}");
            IsEmpty = true;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Refreshes the favorites list.
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        IsRefreshing = true;
        LoadFavorites();
    }

    /// <summary>
    /// Removes a recipe from favorites.
    /// </summary>
    [RelayCommand]
    private void RemoveFavorite(Recipe? recipe)
    {
        if (recipe == null)
            return;

        try
        {
            _recipeService.ToggleFavorite(recipe.Id);
            recipe.IsFavorite = false;
            FavoriteRecipes.Remove(recipe);
            IsEmpty = FavoriteRecipes.Count == 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RemoveFavorite error: {ex.Message}");
        }
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
    /// Clears all favorites with user confirmation.
    /// </summary>
    [RelayCommand]
    private async Task ClearAllFavoritesAsync()
    {
        if (FavoriteRecipes.Count == 0)
            return;

        try
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Clear Favorites",
                "Are you sure you want to remove all favorites?",
                "Yes, Clear All",
                "Cancel");

            if (!confirm)
                return;

            foreach (var recipe in FavoriteRecipes.ToList())
            {
                _recipeService.ToggleFavorite(recipe.Id);
            }
            FavoriteRecipes.Clear();
            IsEmpty = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClearAllFavorites error: {ex.Message}");
        }
    }
}
