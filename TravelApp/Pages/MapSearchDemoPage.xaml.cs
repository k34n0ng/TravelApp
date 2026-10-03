using TravelApp.Controls;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp.Pages;

public partial class MapSearchDemoPage : ContentPage
{
    private readonly IPlaceSearchService _placeSearchService;

    public MapSearchDemoPage()
    {
        InitializeComponent();

        _placeSearchService = Application.Current?.Handler?.MauiContext?.Services
            .GetService<IPlaceSearchService>()
            ?? throw new InvalidOperationException(
                "IPlaceSearchService not registered.");
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

            var results = await _placeSearchService.SearchAsync(
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
                $"Found: {results[0].Name} ({results[0].Provider})";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
            MapHost.Place = null;
        }
    }
}