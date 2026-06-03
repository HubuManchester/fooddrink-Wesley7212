namespace FlavorQuest.Helpers;

/// <summary>
/// Application-wide constants for configuration and keys.
/// </summary>
public static class Constants
{
    // Preference keys for persistent settings
    public const string DarkModeKey = "dark_mode_enabled";
    public const string FontSizeKey = "font_size_preference";
    public const string FavoritesKey = "favorite_recipes";
    public const string LastScanKey = "last_scanned_product";

    // Default values
    public const double DefaultFontSize = 16.0;
    public const double MinFontSize = 12.0;
    public const double MaxFontSize = 24.0;
    public const double FontSizeStep = 2.0;

    // Location defaults
    public const double DefaultLatitude = 53.4808;  // Manchester
    public const double DefaultLongitude = -2.2426;
    public const double SearchRadiusKm = 50.0;

    // API configuration (for potential future real API usage)
    public const string OpenFoodFactsBaseUrl = "https://world.openfoodfacts.org/api/v0/product/";

    // Accessibility
    public const double MinimumTouchTargetSize = 48.0;
    public const double DefaultAnimationDuration = 300;

    // UI
    public const double CardCornerRadius = 12.0;
    public const double StandardSpacing = 16.0;
    public const double SmallSpacing = 8.0;
    public const double LargeSpacing = 24.0;
}
