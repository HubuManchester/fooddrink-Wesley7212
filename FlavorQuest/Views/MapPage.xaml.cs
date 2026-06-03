using FlavorQuest.ViewModels;

namespace FlavorQuest.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Restaurants.Count == 0)
        {
            await _viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}
