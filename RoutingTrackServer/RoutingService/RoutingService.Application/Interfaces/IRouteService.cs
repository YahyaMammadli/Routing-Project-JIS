using RoutingService.Application.Models;

namespace RoutingService.Application.Interfaces;

public interface IRouteService
{
    Task<MultiProviderRouteResult> CalculateAndLogAsync(RouteCalculationRequest request, CancellationToken cancellationToken = default);
}