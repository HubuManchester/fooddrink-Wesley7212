using FlavorQuest.Models;

namespace FlavorQuest.Services;

/// <summary>
/// Manages GPS/location services for finding nearby restaurants and food places.
/// Uses device geolocation and provides both real and simulated data.
/// </summary>
public class LocationService
{
    private Location? _currentLocation;

    // Simulated restaurant data for Manchester area (for demo/offline use)
    private static readonly List<Restaurant> _mockRestaurants = new()
    {
        new() { Name = "The Italian Kitchen", Address = "24 Oxford Street, Manchester", CuisineType = "Italian", Rating = 4.5, Latitude = 53.4810, Longitude = -2.2430, PhoneNumber = "0161-234-5678", IsOpen = true, PriceLevel = "$$$", PlaceId = "mock_001" },
        new() { Name = "Tokyo Ramen House", Address = "15 Portland Street, Manchester", CuisineType = "Japanese", Rating = 4.3, Latitude = 53.4785, Longitude = -2.2405, PhoneNumber = "0161-345-6789", IsOpen = true, PriceLevel = "$$", PlaceId = "mock_002" },
        new() { Name = "Spice Garden", Address = "8 Curry Mile, Manchester", CuisineType = "Indian", Rating = 4.7, Latitude = 53.4550, Longitude = -2.2270, PhoneNumber = "0161-456-7890", IsOpen = true, PriceLevel = "$$", PlaceId = "mock_003" },
        new() { Name = "Burger & Co", Address = "42 Deansgate, Manchester", CuisineType = "American", Rating = 4.1, Latitude = 53.4830, Longitude = -2.2470, PhoneNumber = "0161-567-8901", IsOpen = true, PriceLevel = "$", PlaceId = "mock_004" },
        new() { Name = "Green Leaf Cafe", Address = "56 Northern Quarter, Manchester", CuisineType = "Vegan", Rating = 4.6, Latitude = 53.4840, Longitude = -2.2360, PhoneNumber = "0161-678-9012", IsOpen = true, PriceLevel = "$$", PlaceId = "mock_005" },
        new() { Name = "La Patisserie", Address = "19 King Street, Manchester", CuisineType = "French", Rating = 4.8, Latitude = 53.4815, Longitude = -2.2455, PhoneNumber = "0161-789-0123", IsOpen = false, PriceLevel = "$$$", PlaceId = "mock_006" },
        new() { Name = "Dragon Palace", Address = "77 Chinatown, Manchester", CuisineType = "Chinese", Rating = 4.4, Latitude = 53.4775, Longitude = -2.2390, PhoneNumber = "0161-890-1234", IsOpen = true, PriceLevel = "$$", PlaceId = "mock_007" },
        new() { Name = "Mediterranean Grill", Address = "33 Piccadilly, Manchester", CuisineType = "Mediterranean", Rating = 4.2, Latitude = 53.4790, Longitude = -2.2385, PhoneNumber = "0161-901-2345", IsOpen = true, PriceLevel = "$$$", PlaceId = "mock_008" },
        new() { Name = "Taco Fiesta", Address = "12 Oxford Road, Manchester", CuisineType = "Mexican", Rating = 4.0, Latitude = 53.4760, Longitude = -2.2420, PhoneNumber = "0161-012-3456", IsOpen = true, PriceLevel = "$", PlaceId = "mock_009" },
        new() { Name = "Brew & Bake", Address = "88 Market Street, Manchester", CuisineType = "Cafe", Rating = 4.5, Latitude = 53.4825, Longitude = -2.2410, PhoneNumber = "0161-123-4567", IsOpen = true, PriceLevel = "$", PlaceId = "mock_010" }
    };

    /// <summary>
    /// Gets the current device location asynchronously.
    /// Uses device GPS with a fallback to the default location.
    /// </summary>
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    // Fallback to default location
                    return new Location(Helpers.Constants.DefaultLatitude, Helpers.Constants.DefaultLongitude);
                }
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            _currentLocation = await Geolocation.GetLocationAsync(request);
            return _currentLocation ?? new Location(Helpers.Constants.DefaultLatitude, Helpers.Constants.DefaultLongitude);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            return new Location(Helpers.Constants.DefaultLatitude, Helpers.Constants.DefaultLongitude);
        }
    }

    /// <summary>
    /// Searches for nearby restaurants based on current location.
    /// Calculates distances using the Haversine formula.
    /// </summary>
    /// <param name="cuisineFilter">Optional cuisine type filter.</param>
    /// <returns>List of restaurants sorted by distance.</returns>
    public async Task<List<Restaurant>> GetNearbyRestaurantsAsync(string? cuisineFilter = null)
    {
        var location = await GetCurrentLocationAsync();
        var userLat = location?.Latitude ?? Helpers.Constants.DefaultLatitude;
        var userLon = location?.Longitude ?? Helpers.Constants.DefaultLongitude;

        // Calculate distance for each restaurant
        foreach (var restaurant in _mockRestaurants)
        {
            restaurant.DistanceKm = CalculateDistance(userLat, userLon, restaurant.Latitude, restaurant.Longitude);
        }

        var results = _mockRestaurants
            .Where(r => r.DistanceKm <= Helpers.Constants.SearchRadiusKm)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(cuisineFilter))
        {
            results = results.Where(r =>
                r.CuisineType.Equals(cuisineFilter, StringComparison.OrdinalIgnoreCase));
        }

        return results.OrderBy(r => r.DistanceKm).ToList();
    }

    /// <summary>
    /// Gets distinct cuisine types available from all restaurants.
    /// </summary>
    public List<string> GetCuisineTypes()
    {
        return _mockRestaurants
            .Select(r => r.CuisineType)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// Calculates the distance between two geographic coordinates using the Haversine formula.
    /// This provides accurate distance calculation on a sphere (Earth).
    /// </summary>
    /// <returns>Distance in kilometers.</returns>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return Math.Round(EarthRadiusKm * c, 2);
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
