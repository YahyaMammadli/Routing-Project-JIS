namespace RoutingService.WebApi.DTOs;

public class RouteSnapshotResponseDto
{
    public DateTime CapturedAt { get; set; }
    public double DurationInTrafficMinutes { get; set; }
    public double DurationWithoutTrafficMinutes { get; set; }
    public double TrafficLevel { get; set; }
    public double AverageTaxiPrice { get; set; }
    public double DistanceKm { get; set; }
}