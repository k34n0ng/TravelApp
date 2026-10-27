namespace TravelApp.Services;

/// <summary>
/// Supplies the existing Google API key (configured via GoogleMaps.Local.props)
/// and any platform headers required by API-key application restrictions.
/// Implementations must never log or expose the key.
/// </summary>
public interface IGoogleApiCredentialProvider
{
    string? GetApiKey();

    IReadOnlyDictionary<string, string> GetRequestHeaders();
}
