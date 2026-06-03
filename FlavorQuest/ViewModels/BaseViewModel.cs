using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FlavorQuest.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common functionality
/// such as busy state tracking, title management, and back navigation.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>
    /// Inverse of IsBusy for convenient binding to activity indicators.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Navigates back to the previous page using Shell navigation.
    /// </summary>
    [RelayCommand]
    private async Task GoBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GoBack error: {ex.Message}");
        }
    }
}
