namespace RoutingService.Application.Models;

public class ProviderRouteResult
{
    public string Provider { get; init; } = string.Empty;
    public string TransportMode { get; init; } = string.Empty;

    // Kept for compatibility with the existing RouteLogs table.
    // These values represent the primary (first) route.
    public double DistanceKm { get; init; }
    public double DurationMinutes { get; init; }
    public double? DurationWithoutTrafficMinutes { get; init; }

    public bool TrafficAvailable { get; init; }
    public bool TrafficUsed { get; init; }

    // All route options returned by the provider.
    public List<RouteOption> Routes { get; init; } = [];

    public string? Error { get; init; }
}
