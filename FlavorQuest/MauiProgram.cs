using CommunityToolkit.Maui;
using FlavorQuest.Services;
using FlavorQuest.ViewModels;
using FlavorQuest.Views;
using Microsoft.Extensions.Logging;

namespace FlavorQuest;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services as singletons (shared state across the app)
        builder.Services.AddSingleton<RecipeService>();
        builder.Services.AddSingleton<BarcodeService>();
        builder.Services.AddSingleton<LocationService>();
        builder.Services.AddSingleton<TextToSpeechService>();
        builder.Services.AddSingleton<SettingsService>();

        // Register ViewModels as transient (new instance per navigation)
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<RecipeListViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<FavoritesViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register Pages as transient
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<RecipeListPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
