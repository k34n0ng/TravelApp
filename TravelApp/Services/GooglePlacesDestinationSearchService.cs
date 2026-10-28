using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TravelApp.Models;

namespace TravelApp.Services;

/// <summary>
/// Live destination search using Google Places API (New) Text Search with the
/// existing Google API key. Text Search needs no location context, so results
/// are not biased towards any location. The API key is sent only as a request
/// header and is never logged or included in exception messages.
/// </summary>
public sealed class GooglePlacesDestinationSearchService : IDestinationSearchService
{
    private const string SearchTextEndpoint = "https://places.googleapis.com/v1/places:searchText";
    private const string FieldMask =
        "places.id,places.displayName,places.formattedAddress,places.location,places.addressComponents,places.primaryType";
    private const string NotConfiguredMessage = "Google API key is not configured";

    private readonly IGoogleApiCredentialProvider _credentialProvider;
    private readonly HttpClient _httpClient;

    public GooglePlacesDestinationSearchService(IGoogleApiCredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<IReadOnlyList<MapPlace>> SearchAsync(
        MapSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var apiKey = _credentialProvider.GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new DestinationSearchNotConfiguredException(NotConfiguredMessage);
        }

        var query = request.Query?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            return Array.Empty<MapPlace>();
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, SearchTextEndpoint)
        {
            Content = new StringContent(
                BuildRequestBody(query, Math.Clamp(request.MaxResults, 1, 20)),
                Encoding.UTF8,
                "application/json")
        };

        httpRequest.Headers.Add("X-Goog-Api-Key", apiKey);
        httpRequest.Headers.Add("X-Goog-FieldMask", FieldMask);
        foreach (var header in _credentialProvider.GetRequestHeaders())
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var body = await SendAsync(httpRequest, cancellationToken);
        return ParsePlaces(body);
    }

    private static string BuildRequestBody(string query, int pageSize)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("textQuery", query);
            writer.WriteNumber("pageSize", pageSize);
            writer.WriteString("languageCode", "en");
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private async Task<string> SendAsync(
        HttpRequestMessage httpRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var (status, message) = TryReadError(body);

                // Google error messages describe key/restriction problems but never echo the key.
                Debug.WriteLine(
                    $"GooglePlacesDestinationSearchService: HTTP {(int)response.StatusCode} {status}: {message}");

                throw new DestinationSearchException(
                    $"Google Places request failed ({(int)response.StatusCode}{(status != null ? " " + status : string.Empty)}).");
            }

            return body;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout surfaces as cancellation; report it as a failure, not a user cancel.
            throw new DestinationSearchException("Google Places request timed out.");
        }
        catch (HttpRequestException ex)
        {
            // Transport failure; only the status (if any) is reported, never request details.
            throw new DestinationSearchException(
                ex.StatusCode is { } statusCode
                    ? $"Google Places request failed ({(int)statusCode})."
                    : "Google Places request failed. Check the network connection.");
        }
    }

    private static (string? Status, string? Message) TryReadError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object)
            {
                return (GetString(error, "status"), GetString(error, "message"));
            }
        }
        catch (JsonException)
        {
        }

        return (null, null);
    }

    private static IReadOnlyList<MapPlace> ParsePlaces(string body)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            throw new DestinationSearchException("Google Places returned malformed JSON.");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new DestinationSearchException("Google Places returned an unexpected response.");
            }

            // Text Search returns an empty object when nothing matches.
            if (!root.TryGetProperty("places", out var places))
            {
                return Array.Empty<MapPlace>();
            }

            if (places.ValueKind != JsonValueKind.Array)
            {
                throw new DestinationSearchException("Google Places returned an unexpected response.");
            }

            var results = new List<MapPlace>();
            foreach (var item in places.EnumerateArray())
            {
                var place = TryParsePlace(item);
                if (place != null)
                {
                    results.Add(place);
                }
            }

            return results;
        }
    }

    private static MapPlace? TryParsePlace(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        string? name = null;
        if (item.TryGetProperty("displayName", out var displayName) && displayName.ValueKind == JsonValueKind.Object)
        {
            name = GetString(displayName, "text");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var location = TryParseLocation(item);
        if (location == null)
        {
            return null;
        }

        string? countryCode = null;
        string? city = null;
        if (item.TryGetProperty("addressComponents", out var components) && components.ValueKind == JsonValueKind.Array)
        {
            foreach (var component in components.EnumerateArray())
            {
                if (component.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (countryCode == null && HasType(component, "country"))
                {
                    countryCode = NormalizeCountryCode(GetString(component, "shortText"));
                }
                else if (city == null && HasType(component, "locality"))
                {
                    city = GetString(component, "longText");
                }
            }
        }

        // Provider is assigned by the caller from the country policy; no provider place ID or deep link.
        return new MapPlace
        {
            Name = name.Trim(),
            Address = GetString(item, "formattedAddress"),
            City = city,
            CountryCode = countryCode,
            Category = GetString(item, "primaryType"),
            Location = location
        };
    }

    private static GeoCoordinate? TryParseLocation(JsonElement item)
    {
        if (!item.TryGetProperty("location", out var location) ||
            location.ValueKind != JsonValueKind.Object ||
            !location.TryGetProperty("latitude", out var latElement) ||
            !location.TryGetProperty("longitude", out var lngElement) ||
            latElement.ValueKind != JsonValueKind.Number ||
            lngElement.ValueKind != JsonValueKind.Number ||
            !latElement.TryGetDouble(out var lat) ||
            !lngElement.TryGetDouble(out var lng))
        {
            return null;
        }

        // Validate before constructing GeoCoordinate, which throws on out-of-range values.
        if (!double.IsFinite(lat) || !double.IsFinite(lng) ||
            lat < -90 || lat > 90 || lng < -180 || lng > 180)
        {
            return null;
        }

        return new GeoCoordinate(lat, lng);
    }

    /// <summary>
    /// Accepts only a two-letter A–Z code (ISO 3166-1 alpha-2); anything else is unknown.
    /// </summary>
    private static string? NormalizeCountryCode(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (normalized is not { Length: 2 } ||
            !char.IsAsciiLetterUpper(normalized[0]) ||
            !char.IsAsciiLetterUpper(normalized[1]))
        {
            return null;
        }

        return normalized;
    }

    private static bool HasType(JsonElement component, string type)
    {
        if (!component.TryGetProperty("types", out var types) || types.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var t in types.EnumerateArray())
        {
            if (t.ValueKind == JsonValueKind.String && string.Equals(t.GetString(), type, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
