using TravelApp.ViewModels;

namespace TravelApp.Views;

public partial class MapSearchDemoPage : ContentPage
{
    public MapSearchDemoPage(MapSearchDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}