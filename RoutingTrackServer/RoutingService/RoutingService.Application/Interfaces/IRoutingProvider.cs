using RoutingService.Application.Models;
using RoutingService.Domain.Enums;

namespace RoutingService.Application.Interfaces;

public interface IRoutingProvider
{
    string Name { get; }

    Task<ProviderRouteResult> CalculateAsync(
        RouteCalculationRequest request,
        TransportMode transportMode,
        CancellationToken cancellationToken = default);
}
