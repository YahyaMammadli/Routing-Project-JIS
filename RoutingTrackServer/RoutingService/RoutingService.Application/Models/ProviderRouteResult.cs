namespace RoutingService.Application.Models;

public class ProviderRouteResult
{
    public string Provider { get; init; } = string.Empty;
    public string TransportMode { get; init; } = string.Empty;

    // Primary route values are kept for backward compatibility with the existing API/database logging.
    public double DistanceKm { get; init; }
    public double DurationMinutes { get; init; }
    public double? DurationWithoutTrafficMinutes { get; init; }

    public bool TrafficAvailable { get; init; }
    public bool TrafficUsed { get; init; }

    public List<RouteOption> Routes { get; init; } = [];

    public string? Error { get; init; }
}
