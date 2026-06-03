using FlavorQuest.ViewModels;

namespace FlavorQuest.Views;

public partial class RecipeListPage : ContentPage
{
    public RecipeListPage(RecipeListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
