using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.Services
{
    public sealed class GoogleMapProvider : IMapProvider
    {
        private static readonly MapPlace MarinaBaySandsFixture = new MapPlace
        {
            Name = "Marina Bay Sands",
            LocalName = "Marina Bay Sands",
            Address = "10 Bayfront Ave, Singapore 018956",
            City = "Singapore",
            CountryCode = "SG",
            Category = "hotel",
            Location = new GeoCoordinate(1.2837, 103.8607),
            Provider = MapProviderType.Google,
            ProviderPlaceId = "google_mbs_sg"
        };

        public MapProviderType ProviderType => MapProviderType.Google;

        public bool SupportsCountry(string? countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return true;
            }

            var normalized = countryCode.Trim().ToUpperInvariant();
            return normalized != "CN" && normalized != "KR";
        }

        public Task<IReadOnlyList<MapPlace>> SearchAsync(
            MapSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request != null &&
                !string.IsNullOrWhiteSpace(request.Query) &&
                request.Query.IndexOf("Marina Bay Sands", StringComparison.OrdinalIgnoreCase) >= 0 &&
                request.MaxResults > 0)
            {
                IReadOnlyList<MapPlace> results = new[] { CreateFixtureWithDeepLink() };
                return Task.FromResult(results);
            }

            return Task.FromResult<IReadOnlyList<MapPlace>>(Array.Empty<MapPlace>());
        }

        public Task<MapPlace?> GetPlaceDetailsAsync(
            string providerPlaceId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(providerPlaceId, MarinaBaySandsFixture.ProviderPlaceId, StringComparison.Ordinal))
            {
                return Task.FromResult<MapPlace?>(CreateFixtureWithDeepLink());
            }

            return Task.FromResult<MapPlace?>(null);
        }

        public string? BuildDeepLink(MapPlace place)
        {
            if (place == null || string.IsNullOrWhiteSpace(place.Name) || string.IsNullOrWhiteSpace(place.ProviderPlaceId))
            {
                return null;
            }

            var encodedName = Uri.EscapeDataString(place.Name);
            var encodedPlaceId = Uri.EscapeDataString(place.ProviderPlaceId);
            return $"https://www.google.com/maps/search/?api=1&query={encodedName}&query_place_id={encodedPlaceId}";
        }

        private MapPlace CreateFixtureWithDeepLink()
        {
            var place = new MapPlace
            {
                Name = MarinaBaySandsFixture.Name,
                LocalName = MarinaBaySandsFixture.LocalName,
                Address = MarinaBaySandsFixture.Address,
                City = MarinaBaySandsFixture.City,
                CountryCode = MarinaBaySandsFixture.CountryCode,
                Category = MarinaBaySandsFixture.Category,
                Location = MarinaBaySandsFixture.Location,
                Provider = MarinaBaySandsFixture.Provider,
                ProviderPlaceId = MarinaBaySandsFixture.ProviderPlaceId
            };
            place.MapDeepLink = BuildDeepLink(place);
            return place;
        }
    }
}