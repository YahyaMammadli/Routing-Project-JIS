namespace RoutingService.Application.Models;

public class RouteOption
{
    public int RouteNumber { get; init; }
    public string? RouteId { get; init; }
    public double DistanceKm { get; init; }
    public double DurationMinutes { get; init; }
    public double? DurationWithoutTrafficMinutes { get; init; }
    public string? Algorithm { get; init; }
}
