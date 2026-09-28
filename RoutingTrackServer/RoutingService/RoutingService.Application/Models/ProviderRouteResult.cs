namespace RoutingService.Application.Models;

public class ProviderRouteResult
{
    public string Provider { get; init; } = string.Empty;
    public string TransportMode { get; init; } = string.Empty;
    public double DistanceKm { get; init; } 
    public double DurationMinutes { get; init; }
    public double? DurationWithoutTrafficMinutes { get; init; }
    public bool TrafficAvailable { get; init; }
    public bool TrafficUsed { get; init; }
    public string Error { get; init; } = string.Empty;


}
