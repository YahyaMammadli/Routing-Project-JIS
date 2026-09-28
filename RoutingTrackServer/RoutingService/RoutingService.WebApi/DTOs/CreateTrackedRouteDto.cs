namespace RoutingService.WebApi.DTOs;

public class CreateTrackedRouteDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}