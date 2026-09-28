namespace RoutingService.Domain.Entities;

public class RouteSnapshot
{
    public int Id { get; set; }

    public int TrackedRouteId { get; set; }

    public TrackedRoute? TrackedRoute { get; set; }

    public DateTime CapturedAt { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string TransportMode { get; set; } = string.Empty;

    public double DistanceKm { get; set; }

    public double DurationMinutes { get; set; }

    public double? DurationWithoutTrafficMinutes { get; set; }

    public bool TrafficAvailable { get; set; }

    public bool TrafficUsed { get; set; }
    public double TrafficLevel { get; set; }
    public double AverageTaxiPrice { get; set; }
    public double DurationInTrafficMinutes { get; set; }
}