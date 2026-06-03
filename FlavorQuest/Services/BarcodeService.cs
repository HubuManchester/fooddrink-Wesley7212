using System.Net.Http.Json;
using System.Text.Json;
using FlavorQuest.Models;

namespace FlavorQuest.Services;

/// <summary>
/// Handles barcode scanning and food product lookup.
/// Uses the OpenFoodFacts API for real barcode data with fallback to local mock data.
/// </summary>
public class BarcodeService
{
    private readonly HttpClient _httpClient;
    private static readonly Random _random = new();

    // In-memory mock database of common barcodes for offline/demo use
    private static readonly Dictionary<string, FoodProduct> _mockDatabase = new()
    {
        ["3017620422003"] = new FoodProduct
        {
            Barcode = "3017620422003", ProductName = "Nutella", Brand = "Ferrero",
            ServingSize = "15g", Calories = 80, ProteinGrams = 1.1,
            CarbsGrams = 8.6, FatGrams = 4.7, FiberGrams = 0.5, SugarGrams = 8.5, SodiumMg = 5
        },
        ["5449000000996"] = new FoodProduct
        {
            Barcode = "5449000000996", ProductName = "Coca-Cola Original", Brand = "Coca-Cola",
            ServingSize = "330ml", Calories = 139, ProteinGrams = 0,
            CarbsGrams = 35, FatGrams = 0, FiberGrams = 0, SugarGrams = 35, SodiumMg = 0
        },
        ["5000119487870"] = new FoodProduct
        {
            Barcode = "5000119487870", ProductName = "Heinz Tomato Ketchup", Brand = "Heinz",
            ServingSize = "15ml", Calories = 15, ProteinGrams = 0.2,
            CarbsGrams = 3.5, FatGrams = 0.1, FiberGrams = 0.1, SugarGrams = 3.4, SodiumMg = 180
        },
        ["737628064502"] = new FoodProduct
        {
            Barcode = "737628064502", ProductName = "Barilla Spaghetti", Brand = "Barilla",
            ServingSize = "85g", Calories = 300, ProteinGrams = 10,
            CarbsGrams = 62, FatGrams = 2, FiberGrams = 3, SugarGrams = 2, SodiumMg = 0
        },
        ["8712561495585"] = new FoodProduct
        {
            Barcode = "8712561495585", ProductName = "Oreo Cookies", Brand = "Mondelez",
            ServingSize = "34g", Calories = 160, ProteinGrams = 2,
            CarbsGrams = 25, FatGrams = 7, FiberGrams = 1, SugarGrams = 14, SodiumMg = 90
        },
        ["7622210649405"] = new FoodProduct
        {
            Barcode = "7622210649405", ProductName = "Cadbury Dairy Milk", Brand = "Cadbury",
            ServingSize = "25g", Calories = 134, ProteinGrams = 1.9,
            CarbsGrams = 14.3, FatGrams = 7.6, FiberGrams = 0.5, SugarGrams = 14, SodiumMg = 20
        },
        ["0073991023047"] = new FoodProduct
        {
            Barcode = "0073991023047", ProductName = "Peanut Butter", Brand = "Skippy",
            ServingSize = "32g", Calories = 190, ProteinGrams = 7,
            CarbsGrams = 6, FatGrams = 16, FiberGrams = 2, SugarGrams = 3, SodiumMg = 150
        },
        ["0049000005071"] = new FoodProduct
        {
            Barcode = "0049000005071", ProductName = "Original Ramen Noodles", Brand = "Nissin",
            ServingSize = "85g", Calories = 380, ProteinGrams = 8,
            CarbsGrams = 54, FatGrams = 14, FiberGrams = 3, SugarGrams = 2, SodiumMg = 1820
        }
    };

    public BarcodeService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10),
            BaseAddress = new Uri(Helpers.Constants.OpenFoodFactsBaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FlavorQuest/1.0");
    }

    /// <summary>
    /// Looks up a food product by its barcode number.
    /// Tries the OpenFoodFacts API first, falls back to mock data.
    /// </summary>
    /// <param name="barcode">The barcode string to look up.</param>
    /// <returns>A FoodProduct if found, or null if no match exists.</returns>
    public async Task<FoodProduct?> LookupBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        // Validate barcode format (numeric only, common lengths)
        if (!barcode.All(char.IsDigit) || (barcode.Length != 8 && barcode.Length != 12 && barcode.Length != 13))
        {
            // Try our mock database anyway for known barcodes
            if (_mockDatabase.TryGetValue(barcode, out var knownProduct))
            {
                knownProduct.ScanDate = DateTime.Now;
                return knownProduct;
            }
            return null;
        }

        try
        {
            // Attempt to fetch from OpenFoodFacts API
            var response = await _httpClient.GetAsync($"{barcode}.json");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (json.TryGetProperty("product", out var productElement) &&
                    productElement.TryGetProperty("product_name", out var name) &&
                    !string.IsNullOrEmpty(name.GetString()))
                {
                    return ParseApiResponse(barcode, productElement);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API lookup failed for barcode {barcode}: {ex.Message}");
        }

        // Fallback to mock database
        if (_mockDatabase.TryGetValue(barcode, out var product))
        {
            product.ScanDate = DateTime.Now;
            return product;
        }

        return null;
    }

    /// <summary>
    /// Gets a simulated scan result for demo purposes (when camera is unavailable).
    /// </summary>
    public FoodProduct GetSimulatedScanResult()
    {
        var index = _random.Next(_mockDatabase.Count);
        var product = _mockDatabase.Values.ElementAt(index);
        product.ScanDate = DateTime.Now;
        return product;
    }

    /// <summary>
    /// Parses the OpenFoodFacts API JSON response into a FoodProduct.
    /// </summary>
    private static FoodProduct ParseApiResponse(string barcode, JsonElement product)
    {
        var foodProduct = new FoodProduct
        {
            Barcode = barcode,
            ProductName = GetStringProperty(product, "product_name", "Unknown Product"),
            Brand = GetStringProperty(product, "brands", "Unknown Brand"),
            ServingSize = GetStringProperty(product, "serving_size", "100g"),
            ImageUrl = GetStringProperty(product, "image_url", ""),
            ScanDate = DateTime.Now
        };

        // Parse nutritional values with null safety
        var nutriments = product.TryGetProperty("nutriments", out var n) ? n : default;
        if (nutriments.ValueKind == JsonValueKind.Object)
        {
            foodProduct.Calories = GetDoubleProperty(nutriments, "energy-kcal_100g", 0);
            foodProduct.ProteinGrams = GetDoubleProperty(nutriments, "proteins_100g", 0);
            foodProduct.CarbsGrams = GetDoubleProperty(nutriments, "carbohydrates_100g", 0);
            foodProduct.FatGrams = GetDoubleProperty(nutriments, "fat_100g", 0);
            foodProduct.FiberGrams = GetDoubleProperty(nutriments, "fiber_100g", 0);
            foodProduct.SugarGrams = GetDoubleProperty(nutriments, "sugars_100g", 0);
            foodProduct.SodiumMg = GetDoubleProperty(nutriments, "sodium_100g", 0) * 1000; // Convert g to mg
        }

        return foodProduct;
    }

    /// <summary>
    /// Safely extracts a string property from a JSON element.
    /// </summary>
    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
            return prop.GetString() ?? defaultValue;
        return defaultValue;
    }

    /// <summary>
    /// Safely extracts a double property from a JSON element.
    /// </summary>
    private static double GetDoubleProperty(JsonElement element, string propertyName, double defaultValue)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDouble(out var num))
                return num;
            if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out var parsed))
                return parsed;
        }
        return defaultValue;
    }
}
