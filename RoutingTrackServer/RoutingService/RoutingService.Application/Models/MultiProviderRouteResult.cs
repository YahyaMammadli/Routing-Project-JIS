namespace RoutingService.Application.Models
{
    public class MultiProviderRouteResult
    {
        public string From { get; init; } = string.Empty;
        public string To { get; init; } = string.Empty;
        public DateTime RequestedAt { get; init; }
        public List<ProviderRouteResult> Routes { get; init; } = new()!;

        
    
    
    }
}
