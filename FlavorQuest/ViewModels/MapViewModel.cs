using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the nearby restaurants map page. Uses GPS/location services
/// to find and display nearby food places.
/// </summary>
public partial class MapViewModel : BaseViewModel
{
    private readonly LocationService _locationService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<Restaurant> restaurants = new();

    [ObservableProperty]
    private ObservableCollection<string> cuisineTypes = new();

    [ObservableProperty]
    private string selectedCuisine = "All";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string locationStatus = "Searching for your location...";

    [ObservableProperty]
    private bool hasLocationPermission;

    [ObservableProperty]
    private bool isEmpty;

    public MapViewModel(LocationService locationService, SettingsService settingsService)
    {
        _locationService = locationService;
        _settingsService = settingsService;
        Title = "Nearby Restaurants";
    }

    /// <summary>
    /// Initializes the map page: requests location permission and loads nearby restaurants.
    /// </summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            LocationStatus = "Requesting location permission...";

            // Check and request location permissions with clear error handling
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            HasLocationPermission = status == PermissionStatus.Granted;

            if (!HasLocationPermission)
            {
                LocationStatus = "Location permission denied. Showing default results.";
            }

            // Load restaurants
            await LoadRestaurantsAsync();

            // Load cuisine filter options
            var types = _locationService.GetCuisineTypes();
            types.Insert(0, "All");
            CuisineTypes = new ObservableCollection<string>(types);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MapViewModel init error: {ex.Message}");
            LocationStatus = "Error loading location data.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads nearby restaurants based on current location and optional cuisine filter.
    /// </summary>
    private async Task LoadRestaurantsAsync()
    {
        try
        {
            LocationStatus = "Finding nearby restaurants...";

            var filter = SelectedCuisine == "All" ? null : SelectedCuisine;
            var results = await _locationService.GetNearbyRestaurantsAsync(filter);

            Restaurants = new ObservableCollection<Restaurant>(results);
            IsEmpty = Restaurants.Count == 0;
            LocationStatus = $"Found {results.Count} restaurant(s) nearby.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadRestaurants error: {ex.Message}");
            LocationStatus = "Error loading restaurants. Pull to refresh.";
            IsEmpty = true;
        }
    }

    /// <summary>
    /// Refreshes the restaurant list (pull-to-refresh).
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadRestaurantsAsync();
        IsRefreshing = false;
    }

    /// <summary>
    /// Filters restaurants by the selected cuisine type.
    /// </summary>
    [RelayCommand]
    private async Task FilterByCuisineAsync()
    {
        await _settingsService.TriggerHapticFeedbackAsync();
        await LoadRestaurantsAsync();
    }

    /// <summary>
    /// Opens the phone dialer for the selected restaurant.
    /// </summary>
    [RelayCommand]
    private async Task CallRestaurantAsync(Restaurant? restaurant)
    {
        if (restaurant == null || string.IsNullOrWhiteSpace(restaurant.PhoneNumber))
            return;

        try
        {
            var phoneNumber = restaurant.PhoneNumber.Replace("-", "").Replace(" ", "");
            if (PhoneDialer.IsSupported)
            {
                PhoneDialer.Open(phoneNumber);
            }
            else
            {
                await Clipboard.SetTextAsync(phoneNumber);
                await Shell.Current.DisplayAlert("Phone",
                    $"Phone dialer not available. Number copied to clipboard: {phoneNumber}", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Call error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Could not initiate phone call.", "OK");
        }
    }

    /// <summary>
    /// Opens the map application with directions to the restaurant.
    /// </summary>
    [RelayCommand]
    private async Task OpenInMapAsync(Restaurant? restaurant)
    {
        if (restaurant == null)
            return;

        try
        {
            await _settingsService.TriggerHapticFeedbackAsync();

            var location = new Location(restaurant.Latitude, restaurant.Longitude);
            var options = new MapLaunchOptions
            {
                Name = restaurant.Name,
                NavigationMode = NavigationMode.Driving
            };

            await Map.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map open error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Could not open map application.", "OK");
        }
    }

    /// <summary>
    /// Copies the restaurant address to clipboard.
    /// </summary>
    [RelayCommand]
    private async Task CopyAddressAsync(Restaurant? restaurant)
    {
        if (restaurant == null)
            return;

        try
        {
            await Clipboard.SetTextAsync(restaurant.Address);
            await Shell.Current.DisplayAlert("Copied",
                $"Address copied: {restaurant.Address}", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Copy error: {ex.Message}");
        }
    }
}
