namespace TravelApp.Services
{
    public interface IMapProviderResolver
    {
        IMapProvider Resolve(string? countryCode);
    }
}