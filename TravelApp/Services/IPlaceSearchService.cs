using TravelApp.Models;

namespace TravelApp.Services;

public interface IPlaceSearchService
{
    Task<IReadOnlyList<MapPlace>> SearchAsync(
        MapSearchRequest request,
        CancellationToken cancellationToken = default);
}