namespace TravelApp.Services;

/// <summary>
/// Thrown when a destination search fails (transport error, rejected request,
/// unexpected response, timeout). The message is safe to display and log:
/// it never contains request URLs or credentials.
/// </summary>
public sealed class DestinationSearchException : Exception
{
    public DestinationSearchException(string message)
        : base(message)
    {
    }
}
