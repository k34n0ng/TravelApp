using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp.ViewModels
{
    public class LiveMapSearchViewModel : INotifyPropertyChanged
    {
        private const string UnsupportedProviderStatus = "Provider not supported by MapHostView";
        private const string NoResultsStatus = "No results found";

        private readonly IDestinationSearchService _destinationSearchService;
        private readonly IMapProviderResolver _mapProviderResolver;

        private CancellationTokenSource? _searchCts;

        private string _query = string.Empty;
        private string _status = string.Empty;
        private bool _isBusy;
        private MapPlace? _selectedResult;
        private MapProviderType? _destinationProvider;
        private MapProviderType? _renderedProvider;
        private MapPlace? _displayPlace;
        private MapPlace? _fallbackPlace;

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

        public string Status
        {
            get => _status;
            private set
            {
                if (_status == value)
                    return;

                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MapPlace> Results { get; } = new();

        /// <summary>
        /// The result chosen from <see cref="Results"/>; the first result is selected after a search.
        /// </summary>
        public MapPlace? SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult == value)
                    return;

                _selectedResult = value;
                OnPropertyChanged();
                ApplySelection();
            }
        }

        public string CountryText =>
            _selectedResult == null ? string.Empty : $"Country: {_selectedResult.CountryCode ?? "unknown"}";

        public string ProviderText =>
            _destinationProvider is { } provider ? $"Destination provider: {provider}" : string.Empty;

        public string RendererText =>
            _renderedProvider is { } renderer
                ? $"Map: {renderer}"
                : "Map: Google (fallback)";

        /// <summary>
        /// Place bound to MapHostView; null when the fallback Google map is shown instead.
        /// </summary>
        public MapPlace? DisplayPlace
        {
            get => _displayPlace;
            private set
            {
                if (_displayPlace == value)
                    return;

                _displayPlace = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasRenderedPlace));
                OnPropertyChanged(nameof(ShowFallbackMap));
            }
        }

        public bool HasRenderedPlace => _displayPlace != null;

        public bool ShowFallbackMap => _displayPlace == null;

        public MapPlace? FallbackPlace
        {
            get => _fallbackPlace;
            private set
            {
                if (_fallbackPlace == value)
                    return;

                _fallbackPlace = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FallbackCenter));
            }
        }

        public GeoCoordinate? FallbackCenter => FallbackPlace?.Location;

        public ICommand SearchCommand { get; }
        public Command ShowGoogleCommand { get; }
        public Command ShowNaverCommand { get; }

        public LiveMapSearchViewModel(
            IDestinationSearchService destinationSearchService,
            IMapProviderResolver mapProviderResolver)
        {
            _destinationSearchService = destinationSearchService;
            _mapProviderResolver = mapProviderResolver;

            SearchCommand = new Command(async () => await PerformSearchAsync());
            ShowGoogleCommand = new Command(
                () => Render(MapProviderType.Google),
                () => CanSwitchTo(MapProviderType.Google));
            ShowNaverCommand = new Command(
                () => Render(MapProviderType.Naver),
                () => CanSwitchTo(MapProviderType.Naver));
        }

        private async Task PerformSearchAsync()
        {
            var query = Query?.Trim();
            if (string.IsNullOrEmpty(query))
            {
                ClearResults();
                Status = "Please enter a search query.";
                return;
            }

            _searchCts?.Cancel();
            var cts = new CancellationTokenSource();
            _searchCts = cts;

            IsBusy = true;
            Status = "Searching...";

            try
            {
                var places = await _destinationSearchService.SearchAsync(
                    new MapSearchRequest
                    {
                        Query = query,
                        MaxResults = 5
                    },
                    cts.Token);

                if (cts != _searchCts)
                    return;

                ClearResults();

                if (places.Count == 0)
                {
                    Status = NoResultsStatus;
                    return;
                }

                foreach (var place in places)
                {
                    Results.Add(place);
                }

                SelectedResult = Results[0];
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                // Superseded by a newer search; leave state to that search.
            }
            catch (DestinationSearchNotConfiguredException ex)
            {
                if (cts != _searchCts)
                    return;

                ClearResults();
                Status = ex.Message;
            }
            catch (DestinationSearchException ex)
            {
                if (cts != _searchCts)
                    return;

                System.Diagnostics.Debug.WriteLine($"LiveMapSearchViewModel: {ex.Message}");
                ClearResults();
                Status = $"Search failed. {ex.Message}";
            }
            catch (Exception ex)
            {
                if (cts != _searchCts)
                    return;

                // Unexpected failure: keep the page and fallback map usable.
                System.Diagnostics.Debug.WriteLine($"LiveMapSearchViewModel: unexpected {ex.GetType().Name}");
                ClearResults();
                Status = "Search failed.";
            }
            finally
            {
                if (cts == _searchCts)
                {
                    _searchCts = null;
                    IsBusy = false;
                }

                cts.Dispose();
            }
        }

        private void ClearResults()
        {
            Results.Clear();
            SelectedResult = null;
        }

        private void ApplySelection()
        {
            var place = _selectedResult;

            _destinationProvider = place == null
                ? null
                : _mapProviderResolver.ResolveProviderType(place.CountryCode);

            if (place == null)
            {
                ShowFallback(null);
            }
            else if (_destinationProvider is { } provider && IsRenderable(provider))
            {
                Render(provider);
                Status = $"Found: {place.Name}";
            }
            else
            {
                // Keep the destination provider identity (AMap); show it pinned on the Google fallback.
                ShowFallback(CopyForProvider(place, MapProviderType.AMap));
                Status = UnsupportedProviderStatus;
            }

            OnPropertyChanged(nameof(CountryText));
            OnPropertyChanged(nameof(ProviderText));
            RefreshSwitchCommands();
        }

        /// <summary>
        /// Renders the selected result with the given provider without repeating the search.
        /// </summary>
        private void Render(MapProviderType provider)
        {
            var place = _selectedResult;
            if (place == null || !IsRenderable(provider))
                return;

            _renderedProvider = provider;
            FallbackPlace = null;
            DisplayPlace = CopyForProvider(place, provider);

            OnPropertyChanged(nameof(RendererText));
            RefreshSwitchCommands();
        }

        private void ShowFallback(MapPlace? place)
        {
            _renderedProvider = null;
            DisplayPlace = null;
            FallbackPlace = place;

            OnPropertyChanged(nameof(RendererText));
        }

        private bool CanSwitchTo(MapProviderType provider)
        {
            // Switching is only meaningful for NAVER destinations (South Korea): NAVER <-> Google.
            return _selectedResult != null &&
                   _destinationProvider == MapProviderType.Naver &&
                   _renderedProvider != provider;
        }

        private void RefreshSwitchCommands()
        {
            ShowGoogleCommand.ChangeCanExecute();
            ShowNaverCommand.ChangeCanExecute();
        }

        // Providers MapHostView can currently render (see Controls/MapHostView.cs).
        private static bool IsRenderable(MapProviderType provider)
        {
            return provider == MapProviderType.Google || provider == MapProviderType.Naver;
        }

        private static MapPlace CopyForProvider(MapPlace place, MapProviderType provider)
        {
            return new MapPlace
            {
                Name = place.Name,
                LocalName = place.LocalName,
                Address = place.Address,
                LocalAddress = place.LocalAddress,
                City = place.City,
                CountryCode = place.CountryCode,
                Category = place.Category,
                Location = place.Location,
                Provider = provider,
                ProviderPlaceId = place.ProviderPlaceId,
                MapDeepLink = place.MapDeepLink
            };
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
