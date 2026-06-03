using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlavorQuest.Models;
using FlavorQuest.Services;

namespace FlavorQuest.ViewModels;

/// <summary>
/// ViewModel for the settings page. Manages accessibility features
/// including dark mode toggle, font size adjustment, TTS, haptic feedback,
/// and shake-to-discover preference.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private UserSettings settings;

    [ObservableProperty]
    private string fontSizeDisplay = "Medium";

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        Settings = _settingsService.CurrentSettings;
        Title = "Settings";
        UpdateFontSizeDisplay();

        // Listen for property changes on Settings to apply them immediately
        Settings.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.DarkModeEnabled):
                    _settingsService.SetDarkModeEnabled(Settings.DarkModeEnabled);
                    OnPropertyChanged(nameof(Settings));
                    break;
                case nameof(Settings.TextToSpeechEnabled):
                    _settingsService.SetTextToSpeechEnabled(Settings.TextToSpeechEnabled);
                    break;
                case nameof(Settings.HapticFeedbackEnabled):
                    _settingsService.SetHapticFeedbackEnabled(Settings.HapticFeedbackEnabled);
                    break;
                case nameof(Settings.ShakeToDiscoverEnabled):
                    _settingsService.SetShakeToDiscoverEnabled(Settings.ShakeToDiscoverEnabled);
                    break;
            }
        };
    }

    /// <summary>
    /// Toggles dark mode on/off and applies the theme immediately.
    /// </summary>
    [RelayCommand]
    private void ToggleDarkMode()
    {
        try
        {
            _settingsService.ToggleDarkMode();
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleDarkMode error: {ex.Message}");
        }
    }

    /// <summary>
    /// Increases the font size by one step.
    /// </summary>
    [RelayCommand]
    private void IncreaseFontSize()
    {
        try
        {
            _settingsService.IncreaseFontSize();
            UpdateFontSizeDisplay();
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"IncreaseFontSize error: {ex.Message}");
        }
    }

    /// <summary>
    /// Decreases the font size by one step.
    /// </summary>
    [RelayCommand]
    private void DecreaseFontSize()
    {
        try
        {
            _settingsService.DecreaseFontSize();
            UpdateFontSizeDisplay();
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DecreaseFontSize error: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles text-to-speech on/off.
    /// </summary>
    [RelayCommand]
    private void ToggleTextToSpeech()
    {
        try
        {
            _settingsService.SetTextToSpeechEnabled(!Settings.TextToSpeechEnabled);
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleTTS error: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles haptic feedback on/off.
    /// </summary>
    [RelayCommand]
    private void ToggleHapticFeedback()
    {
        try
        {
            _settingsService.SetHapticFeedbackEnabled(!Settings.HapticFeedbackEnabled);
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleHaptic error: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles shake-to-discover on/off.
    /// </summary>
    [RelayCommand]
    private void ToggleShakeToDiscover()
    {
        try
        {
            _settingsService.SetShakeToDiscoverEnabled(!Settings.ShakeToDiscoverEnabled);
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleShake error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the human-readable font size label.
    /// </summary>
    private void UpdateFontSizeDisplay()
    {
        FontSizeDisplay = Settings.FontSize switch
        {
            <= 12 => "Small",
            <= 16 => "Medium",
            <= 20 => "Large",
            _ => "Extra Large"
        };
    }

    /// <summary>
    /// Resets all settings to defaults with user confirmation.
    /// </summary>
    [RelayCommand]
    private async Task ResetAllSettingsAsync()
    {
        try
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Reset Settings",
                "Are you sure you want to reset all settings to defaults?",
                "Yes, Reset",
                "Cancel");

            if (!confirm)
                return;

            _settingsService.SetDarkModeEnabled(false);
            _settingsService.SetFontSize(Helpers.Constants.DefaultFontSize);
            _settingsService.SetTextToSpeechEnabled(true);
            _settingsService.SetHapticFeedbackEnabled(true);
            _settingsService.SetShakeToDiscoverEnabled(true);
            _settingsService.ApplyTheme();

            Settings = _settingsService.CurrentSettings;
            UpdateFontSizeDisplay();
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ResetSettings error: {ex.Message}");
        }
    }
}
