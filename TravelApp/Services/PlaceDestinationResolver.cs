using System.Threading;
using System.Threading.Tasks;

namespace TravelApp.Services;

public class PlaceDestinationResolver : IPlaceDestinationResolver
{
    public Task<string?> ResolveCountryCodeAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<string?>(null);
        }

        string normalizedQuery = query.ToLowerInvariant();

        if (normalizedQuery.Contains("gyeongbokgung"))
        {
            return Task.FromResult<string?>("KR");
        }
        else if (normalizedQuery.Contains("marina bay sands"))
        {
            return Task.FromResult<string?>("SG");
        }
        else
        {
            return Task.FromResult<string?>(null);
        }
    }
}