using FlavorQuest.Models;

namespace FlavorQuest.Services;

/// <summary>
/// Manages application settings including dark mode, font size, and other user preferences.
/// Settings are persisted using MAUI Preferences and applied immediately.
/// </summary>
public class SettingsService
{
    private UserSettings _currentSettings;

    public UserSettings CurrentSettings => _currentSettings;

    public SettingsService()
    {
        _currentSettings = UserSettings.Load();
    }

    /// <summary>
    /// Toggles dark mode and persists the setting.
    /// </summary>
    public void ToggleDarkMode()
    {
        _currentSettings.DarkModeEnabled = !_currentSettings.DarkModeEnabled;
        _currentSettings.Save();
        ApplyTheme();
    }

    /// <summary>
    /// Sets dark mode to a specific state and persists the setting.
    /// </summary>
    public void SetDarkModeEnabled(bool enabled)
    {
        _currentSettings.DarkModeEnabled = enabled;
        _currentSettings.Save();
        ApplyTheme();
    }

    /// <summary>
    /// Applies the current theme to the application.
    /// </summary>
    public void ApplyTheme()
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = _currentSettings.DarkModeEnabled
                ? AppTheme.Dark
                : AppTheme.Light;
        }
    }

    /// <summary>
    /// Sets the preferred font size and applies it.
    /// </summary>
    public void SetFontSize(double fontSize)
    {
        _currentSettings.FontSize = Math.Clamp(
            fontSize,
            Helpers.Constants.MinFontSize,
            Helpers.Constants.MaxFontSize
        );
        _currentSettings.Save();
    }

    /// <summary>
    /// Increases font size by one step.
    /// </summary>
    public void IncreaseFontSize()
    {
        SetFontSize(_currentSettings.FontSize + Helpers.Constants.FontSizeStep);
    }

    /// <summary>
    /// Decreases font size by one step.
    /// </summary>
    public void DecreaseFontSize()
    {
        SetFontSize(_currentSettings.FontSize - Helpers.Constants.FontSizeStep);
    }

    /// <summary>
    /// Sets whether text-to-speech is enabled.
    /// </summary>
    public void SetTextToSpeechEnabled(bool enabled)
    {
        _currentSettings.TextToSpeechEnabled = enabled;
        _currentSettings.Save();
    }

    /// <summary>
    /// Sets whether haptic feedback is enabled.
    /// </summary>
    public void SetHapticFeedbackEnabled(bool enabled)
    {
        _currentSettings.HapticFeedbackEnabled = enabled;
        _currentSettings.Save();
    }

    /// <summary>
    /// Sets whether shake-to-discover is enabled.
    /// </summary>
    public void SetShakeToDiscoverEnabled(bool enabled)
    {
        _currentSettings.ShakeToDiscoverEnabled = enabled;
        _currentSettings.Save();
    }

    /// <summary>
    /// Triggers haptic feedback if enabled in settings.
    /// </summary>
    public Task TriggerHapticFeedbackAsync()
    {
        if (!_currentSettings.HapticFeedbackEnabled)
            return Task.CompletedTask;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Haptic feedback error: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}
