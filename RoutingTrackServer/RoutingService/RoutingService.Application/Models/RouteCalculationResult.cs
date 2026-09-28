namespace TwoGisRouteServer.Application.Models;

public class RouteCalculationResult
{
    public string CurrentTime { get; } = string.Empty;
    public double DurationInTrafficMinutes { get; }
    public double DurationWithoutTrafficMinutes { get; }
    public double TrafficLevel { get; }
    public double AverageTaxiPrice { get; }
    public double DistanceKm { get; }

    public RouteCalculationResult(
        string currentTime,
        double durationInTrafficMinutes,
        double durationWithoutTrafficMinutes,
        double trafficLevel,
        double averageTaxiPrice,
        double distanceKm)
    {
        CurrentTime = currentTime;
        DurationInTrafficMinutes = durationInTrafficMinutes;
        DurationWithoutTrafficMinutes = durationWithoutTrafficMinutes;
        TrafficLevel = trafficLevel;
        AverageTaxiPrice = averageTaxiPrice;
        DistanceKm = distanceKm;
    }

    public override string ToString()
        => $"RouteCalculationResult {{ " +
           $"CurrentTime = {CurrentTime}, " +
           $"DurationInTrafficMinutes = {DurationInTrafficMinutes}, " +
           $"DurationWithoutTrafficMinutes = {DurationWithoutTrafficMinutes}, " +
           $"TrafficLevel = {TrafficLevel}, " +
           $"AverageTaxiPrice = {AverageTaxiPrice}, " +
           $"DistanceKm = {DistanceKm} }}";
}