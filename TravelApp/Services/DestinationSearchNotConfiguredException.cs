namespace TravelApp.Services;

/// <summary>
/// Thrown when a destination search service is missing required configuration
/// (for example an API key). The message is safe to display to the user.
/// </summary>
public sealed class DestinationSearchNotConfiguredException : Exception
{
    public DestinationSearchNotConfiguredException(string message)
        : base(message)
    {
    }
}
