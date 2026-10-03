using TravelApp.Controls;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp.Pages;

public partial class MapSearchDemoPage : ContentPage
{
    private readonly IMapProviderResolver _resolver;

    public MapSearchDemoPage()
    {
        InitializeComponent();

        _resolver = Application.Current?.Handler?.MauiContext?.Services
            .GetService<IMapProviderResolver>()
            ?? throw new InvalidOperationException(
                "IMapProviderResolver not registered.");
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string? query = QueryEntry.Text;
        string? countryCode = CountryPicker.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(query) ||
            string.IsNullOrWhiteSpace(countryCode))
        {
            ResultLabel.Text = "Invalid input";
            MapHost.Place = null;
            return;
        }

        try
        {
            var request = new MapSearchRequest
            {
                Query = query,
                CountryCode = countryCode
            };

            var provider = _resolver.Resolve(request.CountryCode);

            var results = await provider.SearchAsync(
                request,
                CancellationToken.None);

            if (results.Count == 0)
            {
                ResultLabel.Text = "No results";
                MapHost.Place = null;
                return;
            }

            MapHost.Place = results[0];

            ResultLabel.Text =
                $"Found: {results[0].Name} ({provider.ProviderType})";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
            MapHost.Place = null;
        }
    }
}