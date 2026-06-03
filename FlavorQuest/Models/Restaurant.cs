namespace FlavorQuest.Models;

/// <summary>
/// Represents a nearby restaurant or food place retrieved via GPS/location services.
/// </summary>
public class Restaurant
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CuisineType { get; set; } = string.Empty;
    public double Rating { get; set; }
    public double DistanceKm { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string PriceLevel { get; set; } = "$$";
    public string PlaceId { get; set; } = string.Empty;

    /// <summary>
    /// Formatted distance string for display (e.g., "1.2 km").
    /// </summary>
    public string DistanceDisplay => $"{DistanceKm:F1} km";

    /// <summary>
    /// Formatted rating display with star symbol.
    /// </summary>
    public string RatingDisplay => $"⭐ {Rating:F1}";

    /// <summary>
    /// Color indicator for whether the restaurant is currently open.
    /// </summary>
    public Color OpenStatusColor => IsOpen ? Colors.Green : Colors.Red;

    /// <summary>
    /// Text indicator for whether the restaurant is currently open.
    /// </summary>
    public string OpenStatusText => IsOpen ? "Open" : "Closed";

    public override string ToString() => $"{Name} - {DistanceDisplay}";
}
