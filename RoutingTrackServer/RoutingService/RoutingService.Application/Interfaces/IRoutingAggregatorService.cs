using RoutingService.Application.Models;

namespace RoutingService.Application.Interfaces;

public interface IRoutingAggregatorService
{
    Task<MultiProviderRouteResult> CalculateAsync(
        RouteCalculationRequest request,
        CancellationToken cancellationToken = default);
}