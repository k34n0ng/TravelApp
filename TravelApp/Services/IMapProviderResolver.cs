using TravelApp.Models;

namespace TravelApp.Services
{
    public interface IMapProviderResolver
    {
        IMapProvider Resolve(string? countryCode);

        MapProviderType ResolveProviderType(string? countryCode);
    }
}