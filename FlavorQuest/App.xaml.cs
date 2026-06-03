using FlavorQuest.Services;

namespace FlavorQuest;

public partial class App : Application
{
    private readonly RecipeService _recipeService;
    private readonly SettingsService _settingsService;

    public App(RecipeService recipeService, SettingsService settingsService)
    {
        InitializeComponent();

        _recipeService = recipeService;
        _settingsService = settingsService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            await _recipeService.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Recipe init error: {ex.Message}");
        }

        _settingsService.ApplyTheme();
    }
}
