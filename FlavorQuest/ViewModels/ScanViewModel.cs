using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the barcode scanner page. Uses the device camera
/// to capture barcodes and lookup food product nutritional information.
/// </summary>
public partial class ScanViewModel : BaseViewModel
{
    private readonly BarcodeService _barcodeService;

    [ObservableProperty]
    private string barcodeText = string.Empty;

    [ObservableProperty]
    private FoodProduct? scannedProduct;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private string statusMessage = "Point your camera at a barcode to scan.";

    [ObservableProperty]
    private bool isCameraAvailable;

    public ScanViewModel(BarcodeService barcodeService)
    {
        _barcodeService = barcodeService;
        Title = "Barcode Scanner";
    }

    /// <summary>
    /// Checks camera availability when the page loads.
    /// </summary>
    [RelayCommand]
    private async Task CheckCameraPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }
            IsCameraAvailable = status == PermissionStatus.Granted;

            if (!IsCameraAvailable)
            {
                StatusMessage = "Camera permission is required for barcode scanning.";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Camera permission error: {ex.Message}");
            IsCameraAvailable = false;
            StatusMessage = "Camera unavailable. Use manual barcode entry instead.";
        }
    }

    /// <summary>
    /// Looks up a barcode that was manually entered by the user.
    /// Includes input validation for barcode format.
    /// </summary>
    [RelayCommand]
    private async Task LookupBarcodeAsync()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(BarcodeText))
        {
            await Shell.Current.DisplayAlert("Validation Error",
                "Please enter a barcode number.", "OK");
            return;
        }

        var cleanBarcode = BarcodeText.Trim();
        if (!cleanBarcode.All(char.IsDigit))
        {
            await Shell.Current.DisplayAlert("Validation Error",
                "Barcode must contain only numbers.", "OK");
            return;
        }

        if (cleanBarcode.Length < 8)
        {
            await Shell.Current.DisplayAlert("Validation Error",
                "Barcode is too short. Please enter a valid barcode.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Looking up product...";
            HasResult = false;

            var product = await _barcodeService.LookupBarcodeAsync(cleanBarcode);
            if (product != null)
            {
                ScannedProduct = product;
                HasResult = true;
                StatusMessage = $"Found: {product.ProductName}";
            }
            else
            {
                ScannedProduct = null;
                HasResult = false;
                StatusMessage = $"No product found for barcode: {cleanBarcode}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Barcode lookup error: {ex.Message}");
            StatusMessage = "Error looking up barcode. Please try again.";
            await Shell.Current.DisplayAlert("Error",
                $"Failed to look up the barcode: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Gets a simulated scan result for demonstration purposes.
    /// Useful when running on emulators or when camera is unavailable.
    /// </summary>
    [RelayCommand]
    private void SimulateScan()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Simulating scan...";

            var product = _barcodeService.GetSimulatedScanResult();
            ScannedProduct = product;
            HasResult = true;
            BarcodeText = product.Barcode;
            StatusMessage = $"Found: {product.ProductName}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SimulateScan error: {ex.Message}");
            StatusMessage = "Simulation failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Clears the current scan result and resets the view.
    /// </summary>
    [RelayCommand]
    private void ClearScan()
    {
        ScannedProduct = null;
        HasResult = false;
        BarcodeText = string.Empty;
        StatusMessage = "Point your camera at a barcode to scan.";
    }
}
