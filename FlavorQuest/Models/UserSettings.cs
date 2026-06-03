using CommunityToolkit.Mvvm.ComponentModel;

namespace FlavorQuest.Models;

/// <summary>
/// Holds user-configurable settings for the application.
/// Uses ObservableObject to support live UI updates when settings change.
/// </summary>
public partial class UserSettings : ObservableObject
{
    [ObservableProperty]
    private bool darkModeEnabled;

    [ObservableProperty]
    private double fontSize = 16.0;

    [ObservableProperty]
    private bool textToSpeechEnabled = true;

    [ObservableProperty]
    private bool hapticFeedbackEnabled = true;

    [ObservableProperty]
    private bool shakeToDiscoverEnabled = true;

    /// <summary>
    /// Loads settings from device preferences.
    /// </summary>
    public static UserSettings Load()
    {
        return new UserSettings
        {
            DarkModeEnabled = Preferences.Get(Helpers.Constants.DarkModeKey, false),
            FontSize = Preferences.Get(Helpers.Constants.FontSizeKey, Helpers.Constants.DefaultFontSize),
            TextToSpeechEnabled = Preferences.Get("tts_enabled", true),
            HapticFeedbackEnabled = Preferences.Get("haptic_enabled", true),
            ShakeToDiscoverEnabled = Preferences.Get("shake_enabled", true)
        };
    }

    /// <summary>
    /// Saves current settings to device preferences.
    /// </summary>
    public void Save()
    {
        Preferences.Set(Helpers.Constants.DarkModeKey, DarkModeEnabled);
        Preferences.Set(Helpers.Constants.FontSizeKey, FontSize);
        Preferences.Set("tts_enabled", TextToSpeechEnabled);
        Preferences.Set("haptic_enabled", HapticFeedbackEnabled);
        Preferences.Set("shake_enabled", ShakeToDiscoverEnabled);
    }
}
