using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the Home page. Displays categories, featured recipes,
/// a search bar, and quick-action buttons for hardware features.
/// </summary>
public partial class HomeViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private ObservableCollection<Recipe> featuredRecipes = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private bool hasSearchResults;

    [ObservableProperty]
    private bool isRefreshing;

    public HomeViewModel(RecipeService recipeService, SettingsService settingsService)
    {
        _recipeService = recipeService;
        _settingsService = settingsService;
        Title = "FlavorQuest";
    }

    /// <summary>
    /// Initializes the home page data asynchronously. Called when the page appears.
    /// </summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            // Load categories
            var cats = _recipeService.GetCategories();
            Categories = new ObservableCollection<Category>(cats);

            // Load featured recipes (one random recipe per category)
            var featured = new List<Recipe>();
            foreach (var category in cats.Take(4))
            {
                var recipe = _recipeService.GetRandomRecipe(category.Name);
                if (recipe != null)
                    featured.Add(recipe);
            }
            FeaturedRecipes = new ObservableCollection<Recipe>(featured);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HomeViewModel initialization error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Could not load recipes. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Handles pull-to-refresh on the home page.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await InitializeAsync();
    }

    /// <summary>
    /// Navigates to the recipe list filtered by the selected category.
    /// </summary>
    [RelayCommand]
    private async Task GoToCategoryAsync(Category? category)
    {
        if (category == null)
            return;

        await _settingsService.TriggerHapticFeedbackAsync();

        try
        {
            await Shell.Current.GoToAsync($"recipeList?category={Uri.EscapeDataString(category.Name)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to the recipe detail page.
    /// </summary>
    [RelayCommand]
    private async Task GoToRecipeDetailAsync(Recipe? recipe)
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
    /// Executes a search and navigates to the results page.
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await Shell.Current.DisplayAlert("Search", "Please enter a search term.", "OK");
            return;
        }

        try
        {
            var encodedQuery = Uri.EscapeDataString(SearchQuery.Trim());
            await Shell.Current.GoToAsync($"recipeList?search={encodedQuery}");
            SearchQuery = string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to the barcode scanner page.
    /// Uses Camera hardware feature.
    /// </summary>
    [RelayCommand]
    private async Task GoToScanAsync()
    {
        await _settingsService.TriggerHapticFeedbackAsync();
        try
        {
            await Shell.Current.GoToAsync("scanPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to the nearby restaurants map page.
    /// Uses GPS/Location hardware feature.
    /// </summary>
    [RelayCommand]
    private async Task GoToMapAsync()
    {
        await _settingsService.TriggerHapticFeedbackAsync();
        try
        {
            await Shell.Current.GoToAsync("mapPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Shake gesture handler - gets a random recipe suggestion.
    /// Uses Accelerometer hardware feature for shake detection.
    /// </summary>
    public async Task ShakeForRandomRecipeAsync()
    {
        if (!_settingsService.CurrentSettings.ShakeToDiscoverEnabled || IsBusy)
            return;

        try
        {
            IsBusy = true;
            var randomRecipe = _recipeService.GetRandomRecipe();
            if (randomRecipe != null)
            {
                await _settingsService.TriggerHapticFeedbackAsync();
                await Shell.Current.GoToAsync($"recipeDetail?recipeId={randomRecipe.Id}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Shake handler error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
