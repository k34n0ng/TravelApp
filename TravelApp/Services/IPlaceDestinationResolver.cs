using System.Threading;
using System.Threading.Tasks;

namespace TravelApp.Services;

public interface IPlaceDestinationResolver
{
    Task<string?> ResolveCountryCodeAsync(string query, CancellationToken cancellationToken);
}