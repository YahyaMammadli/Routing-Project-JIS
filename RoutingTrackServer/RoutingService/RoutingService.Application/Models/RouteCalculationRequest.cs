namespace RoutingService.Application.Models;

public class RouteCalculationRequest
{
    public string From { get; }

    public string To { get; }

    public RouteCalculationRequest(
        string from,
        string to)
    {
        From = from;
        To = to;
    }
}