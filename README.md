# FlavorQuest

A cross-platform food discovery app built with **.NET MAUI** (Multi-platform App UI).

## Features

| Feature | Hardware Used |
|---------|---------------|
| Recipe browsing with categories | - |
| Recipe search (by name, ingredients, tags) | - |
| Recipe detail with nutrition info | - |
| Text-to-Speech for reading recipes | 🔊 Speaker |
| Barcode scanner for food products | 📷 Camera |
| Nearby restaurant finder | 📍 GPS / Location |
| Shake-to-discover random recipe | 📱 Accelerometer |
| Haptic feedback on interactions | 📳 Vibrator |
| Phone dialer for restaurant calls | 📞 Phone Dialer |
| Map navigation to restaurants | 🗺 Maps |
| Dark mode & font size adjustment | ♿ Accessibility |

## Tech Stack

- **Framework**: .NET MAUI (.NET 9)
- **Language**: C#
- **Architecture**: MVVM (Model-View-ViewModel)
- **Libraries**: CommunityToolkit.Maui, CommunityToolkit.Mvvm
- **Platforms**: Android, iOS, macOS, Windows

## Project Structure

```
FlavorQuest/
├── Models/          # Data models (Recipe, Category, FoodProduct, Restaurant, UserSettings)
├── ViewModels/      # MVVM ViewModels with data binding and commands
├── Views/           # XAML pages with UI definitions
├── Services/        # Business logic (RecipeService, BarcodeService, LocationService, etc.)
├── Helpers/         # Value converters and constants
├── Resources/       # Styles, colors, recipe data (JSON)
└── Platforms/       # Platform-specific code (Android, iOS, Windows, macOS)
```

## How to Run

```bash
# Windows
dotnet build -f net9.0-windows10.0.19041.0
dotnet run -f net9.0-windows10.0.19041.0

# Android (requires Android SDK)
dotnet build -f net9.0-android
```

Or open `FlavorQuest.csproj` in Visual Studio 2022 and press F5.

## Code Quality

- MVVM architecture with dependency injection
- Exception handling in all service calls
- Input validation (barcode format, search query)
- XAML compiled bindings for type safety
- Null-safety enabled (`<Nullable>enable</Nullable>`)
- XML documentation on all public methods
