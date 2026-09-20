using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.Services
{
    public interface IMapProvider
    {
        MapProviderType ProviderType { get; }

        bool SupportsCountry(string? countryCode);

        Task<IReadOnlyList<MapPlace>> SearchAsync(
            MapSearchRequest request,
            CancellationToken cancellationToken = default);

        Task<MapPlace?> GetPlaceDetailsAsync(
            string providerPlaceId,
            CancellationToken cancellationToken = default);

        string? BuildDeepLink(MapPlace place);
    }
}