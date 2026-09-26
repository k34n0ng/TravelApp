using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp
{
    public partial class MainPage : ContentPage
    {
        private readonly IMapProviderResolver _providerResolver;
        private MapPlace? _selectedPlace;

        public MainPage(IMapProviderResolver providerResolver)
        {
            InitializeComponent();
            _providerResolver = providerResolver ?? throw new ArgumentNullException(nameof(providerResolver));
        }

        private async void OnSearchClicked(object? sender, EventArgs e)
        {
            var searchText = SearchEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ShowStatus("Please enter a search query.");
                return;
            }

            try
            {
                StatusLabel.IsVisible = false;
                ResultCard.IsVisible = false;
                _selectedPlace = null;

                var provider = _providerResolver.Resolve("SG");
                var request = new MapSearchRequest
                {
                    Query = searchText,
                    CountryCode = "SG",
                    MaxResults = 10
                };

                var results = await provider.SearchAsync(request);
                var place = results.FirstOrDefault();

                if (place != null)
                {
                    _selectedPlace = place;
                    PlaceNameLabel.Text = place.Name;
                    PlaceAddressLabel.Text = place.Address ?? place.City ?? string.Empty;
                    PlaceProviderLabel.Text = $"Provider: {place.Provider}";

                    if (!string.IsNullOrWhiteSpace(place.MapDeepLink))
                    {
                        OpenDeepLinkButton.IsVisible = true;
                    }
                    else
                    {
                        OpenDeepLinkButton.IsVisible = false;
                    }

                    ResultCard.IsVisible = true;

                    if (place.Location != null)
                    {
                        MapViewControl.InitialCenter = place.Location;
                        MapViewControl.InitialZoom = 14;
                    }

                    MapViewControl.SelectedPlace = place;
                }
                else
                {
                    ShowStatus("No places found.");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Search error: {ex.Message}");
            }
        }

        private async void OnOpenDeepLinkClicked(object? sender, EventArgs e)
        {
            if (_selectedPlace != null && !string.IsNullOrWhiteSpace(_selectedPlace.MapDeepLink))
            {
                try
                {
                    if (Uri.TryCreate(_selectedPlace.MapDeepLink, UriKind.Absolute, out var uri))
                    {
                        await Launcher.Default.OpenAsync(uri);
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"Could not open map link: {ex.Message}");
                }
            }
        }

        private void ShowStatus(string message)
        {
            StatusLabel.Text = message;
            StatusLabel.IsVisible = true;
        }
    }
}
