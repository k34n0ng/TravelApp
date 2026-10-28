namespace TravelApp.Services;

/// <summary>
/// Reads the existing Google API key from platform configuration.
/// Platform parts live under Platforms/; platforms without an implementation
/// report no key, which callers surface as "not configured".
/// </summary>
public sealed partial class GoogleApiCredentialProvider : IGoogleApiCredentialProvider
{
    private readonly Lazy<string?> _apiKey;
    private readonly Lazy<IReadOnlyDictionary<string, string>> _requestHeaders;

    public GoogleApiCredentialProvider()
    {
        _apiKey = new Lazy<string?>(() =>
        {
            string? apiKey = null;
            ReadPlatformApiKey(ref apiKey);
            return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
        });

        _requestHeaders = new Lazy<IReadOnlyDictionary<string, string>>(() =>
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddPlatformRequestHeaders(headers);
            return headers;
        });
    }

    public string? GetApiKey() => _apiKey.Value;

    public IReadOnlyDictionary<string, string> GetRequestHeaders() => _requestHeaders.Value;

    partial void ReadPlatformApiKey(ref string? apiKey);

    partial void AddPlatformRequestHeaders(IDictionary<string, string> headers);
}
