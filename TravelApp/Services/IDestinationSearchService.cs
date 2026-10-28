using TravelApp.Models;

namespace TravelApp.Services;

/// <summary>
/// Live, provider-agnostic destination search. Returned places carry the
/// destination country (ISO 3166-1 alpha-2) and location; the map renderer
/// is chosen separately via <see cref="IMapProviderResolver"/>.
/// </summary>
public interface IDestinationSearchService
{
    Task<IReadOnlyList<MapPlace>> SearchAsync(
        MapSearchRequest request,
        CancellationToken cancellationToken = default);
}
