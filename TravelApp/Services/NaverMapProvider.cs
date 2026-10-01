using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.Services
{
    /// <summary>
    /// IMapProvider implementation for NAVER Maps (South Korea).
    /// Currently provides a deterministic fixture for Gyeongbokgung Palace
    /// to validate the native NAVER map rendering pipeline.
    /// </summary>
    public sealed class NaverMapProvider : IMapProvider
    {
        /// <summary>
        /// Gyeongbokgung Palace test fixture.
        /// WGS84 coordinates: 37.5796, 126.9770.
        /// </summary>
        private static readonly MapPlace GyeongbokgungFixture = new MapPlace
        {
            Name = "Gyeongbokgung Palace",
            LocalName = "경복궁",
            Address = "161 Sajik-ro, Jongno-gu, Seoul, South Korea",
            LocalAddress = "서울특별시 종로구 사직로 161",
            City = "Seoul",
            CountryCode = "KR",
            Category = "landmark",
            Location = new GeoCoordinate(37.5796, 126.9770),
            Provider = MapProviderType.Naver,
            ProviderPlaceId = "naver_gyeongbokgung_kr"
        };

        public MapProviderType ProviderType => MapProviderType.Naver;

        public bool SupportsCountry(string? countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return false;
            }

            var normalized = countryCode.Trim().ToUpperInvariant();
            return normalized == "KR";
        }

        public Task<IReadOnlyList<MapPlace>> SearchAsync(
            MapSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request != null &&
                !string.IsNullOrWhiteSpace(request.Query) &&
                request.Query.IndexOf("Gyeongbokgung", StringComparison.OrdinalIgnoreCase) >= 0 &&
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

            if (string.Equals(providerPlaceId, GyeongbokgungFixture.ProviderPlaceId, StringComparison.Ordinal))
            {
                return Task.FromResult<MapPlace?>(CreateFixtureWithDeepLink());
            }

            return Task.FromResult<MapPlace?>(null);
        }

        public string? BuildDeepLink(MapPlace place)
        {
            if (place?.Location == null || string.IsNullOrWhiteSpace(place.Name))
            {
                return null;
            }

            var encodedName = Uri.EscapeDataString(place.Name);
            return $"https://map.naver.com/v5/search/{encodedName}?c={place.Location.Longitude},{place.Location.Latitude},15,0,0,0,dh";
        }

        private MapPlace CreateFixtureWithDeepLink()
        {
            var place = new MapPlace
            {
                Name = GyeongbokgungFixture.Name,
                LocalName = GyeongbokgungFixture.LocalName,
                Address = GyeongbokgungFixture.Address,
                LocalAddress = GyeongbokgungFixture.LocalAddress,
                City = GyeongbokgungFixture.City,
                CountryCode = GyeongbokgungFixture.CountryCode,
                Category = GyeongbokgungFixture.Category,
                Location = GyeongbokgungFixture.Location,
                Provider = GyeongbokgungFixture.Provider,
                ProviderPlaceId = GyeongbokgungFixture.ProviderPlaceId
            };
            place.MapDeepLink = BuildDeepLink(place);
            return place;
        }
    }
}
