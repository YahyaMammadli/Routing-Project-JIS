namespace RoutingService.Domain.Entities;

public class RouteLog
{
    public int Id { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string TransportMode { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public double DistanceKm { get; set; }
    public double DurationMinutes { get; set; }
    public double? DurationWithoutTrafficMinutes { get; set; }
    public bool TrafficAvailable { get; set; }
    public bool TrafficUsed { get; set; }
    public string? Error { get; set; }
}