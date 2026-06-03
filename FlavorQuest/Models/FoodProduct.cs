namespace FlavorQuest.Models;

/// <summary>
/// Represents a food product identified by barcode scanning, with nutritional data.
/// </summary>
public class FoodProduct
{
    public string Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string ServingSize { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public double FiberGrams { get; set; }
    public double SugarGrams { get; set; }
    public double SodiumMg { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime ScanDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Returns a formatted nutritional summary for display.
    /// </summary>
    public string NutritionSummary =>
        $"Calories: {Calories} | Protein: {ProteinGrams}g | Carbs: {CarbsGrams}g | Fat: {FatGrams}g";

    public override string ToString() => $"{ProductName} ({Brand})";
}
