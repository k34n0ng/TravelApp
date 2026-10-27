using TravelApp.ViewModels;

namespace TravelApp.Views;

public partial class LiveMapSearchPage : ContentPage
{
    public LiveMapSearchPage(LiveMapSearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
