using TravelApp.Models;

namespace TravelApp.Services;

public sealed class PlaceSearchService : IPlaceSearchService
{
    private readonly IMapProviderResolver _resolver;

    public PlaceSearchService(IMapProviderResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<IReadOnlyList<MapPlace>> SearchAsync(
        MapSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = _resolver.Resolve(request.CountryCode);

        return await provider.SearchAsync(
            request,
            cancellationToken);
    }
}