using FlavorQuest.ViewModels;

namespace FlavorQuest.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.FeaturedRecipes.Count == 0)
        {
            await _viewModel.InitializeCommand.ExecuteAsync(null);
        }

        StartShakeDetection();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShakeDetection();
    }

    private void StartShakeDetection()
    {
        try
        {
            if (Accelerometer.IsSupported)
            {
                Accelerometer.ShakeDetected += OnShakeDetected;
                Accelerometer.Start(SensorSpeed.Game);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accelerometer error: {ex.Message}");
        }
    }

    private void StopShakeDetection()
    {
        try
        {
            if (Accelerometer.IsSupported)
            {
                Accelerometer.ShakeDetected -= OnShakeDetected;
                Accelerometer.Stop();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accelerometer stop error: {ex.Message}");
        }
    }

    private async void OnShakeDetected(object? sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _viewModel.ShakeForRandomRecipeAsync();
        });
    }
}
