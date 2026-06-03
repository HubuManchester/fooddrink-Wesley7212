using FlavorQuest.Services;
using FlavorQuest.ViewModels;

namespace FlavorQuest.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly SettingsService _settingsService;

    public SettingsPage(SettingsViewModel viewModel, SettingsService settingsService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _settingsService = settingsService;
        BindingContext = viewModel;
    }

    private void OnDarkModeToggled(object? sender, ToggledEventArgs e)
    {
        // IsToggled binding already updated the property, just apply the theme
        _settingsService.SetDarkModeEnabled(e.Value);
    }

    private void OnTtsToggled(object? sender, ToggledEventArgs e)
    {
        _settingsService.SetTextToSpeechEnabled(e.Value);
    }

    private void OnHapticToggled(object? sender, ToggledEventArgs e)
    {
        _settingsService.SetHapticFeedbackEnabled(e.Value);
    }

    private void OnShakeToggled(object? sender, ToggledEventArgs e)
    {
        _settingsService.SetShakeToDiscoverEnabled(e.Value);
    }
}
