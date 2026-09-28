namespace TwoGisRouteServer.API.DTOs;

public class RouteInfoResponseDto
{
    public string CurrentTime { get; set; } = string.Empty;
    public double DurationInTrafficMinutes { get; set; }
    public double DurationWithoutTrafficMinutes { get; set; }
    public double TrafficLevel { get; set; }
    public double AverageTaxiPrice { get; set; }
    public double DistanceKm { get; set; }
}