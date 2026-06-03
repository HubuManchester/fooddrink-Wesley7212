using FlavorQuest.Views;

namespace FlavorQuest;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("recipeList", typeof(RecipeListPage));
        Routing.RegisterRoute("recipeDetail", typeof(RecipeDetailPage));
        Routing.RegisterRoute("scanPage", typeof(ScanPage));
        Routing.RegisterRoute("mapPage", typeof(MapPage));
    }
}
