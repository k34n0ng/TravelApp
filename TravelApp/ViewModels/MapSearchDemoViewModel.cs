using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp.ViewModels
{
    public class MapSearchDemoViewModel : INotifyPropertyChanged
    {
        private readonly IPlaceDestinationResolver _placeDestinationResolver;
        private readonly IPlaceSearchService _placeSearchService;

        private string _query = string.Empty;
        private string _result = string.Empty;
        private MapPlace? _selectedPlace;

        public string Query
        {
            get => _query;
            set
            {
                if (_query == value)
                    return;

                _query = value;
                OnPropertyChanged();
            }
        }

        public string Result
        {
            get => _result;
            private set
            {
                if (_result == value)
                    return;

                _result = value;
                OnPropertyChanged();
            }
        }

        public MapPlace? SelectedPlace
        {
            get => _selectedPlace;
            private set
            {
                if (_selectedPlace == value)
                    return;

                _selectedPlace = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }

        public MapSearchDemoViewModel(
            IPlaceDestinationResolver placeDestinationResolver,
            IPlaceSearchService placeSearchService)
        {
            _placeDestinationResolver = placeDestinationResolver;
            _placeSearchService = placeSearchService;

            SearchCommand = new Command(async () => await PerformSearchAsync());
        }

        private async Task PerformSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                Result = "Please enter a search query.";
                SelectedPlace = null;
                return;
            }

            var countryCode =
                await _placeDestinationResolver.ResolveCountryCodeAsync(
                    Query,
                    CancellationToken.None);

            if (countryCode == null)
            {
                Result = "Unable to determine destination.";
                SelectedPlace = null;
                return;
            }

            var request = new MapSearchRequest
{
    Query = Query,
    CountryCode = countryCode
};

var places =
    await _placeSearchService.SearchAsync(
        request,
        CancellationToken.None);

if (places == null || places.Count == 0)
{
    Result = "No results found.";
    SelectedPlace = null;
    return;
}

var place = places[0];

Result = $"Found: {place.Name}";
SelectedPlace = place;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}