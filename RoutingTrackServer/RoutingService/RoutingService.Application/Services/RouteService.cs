using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Entities;
using TwoGisRouteServer.Application.Interfaces;

namespace RoutingService.Application.Services;

public class RouteService(IRoutingAggregatorService routingAggregatorService, IRouteLogRepository routeLogRepository) : IRouteService
{
    public async Task<MultiProviderRouteResult> CalculateAndLogAsync(RouteCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await routingAggregatorService.CalculateAsync(request, cancellationToken);

        foreach (var route in result.Routes)
        {
            var log = new RouteLog
            {
                FromAddress = result.From,
                ToAddress = result.To,

                Provider = route.Provider,
                TransportMode = route.TransportMode,

                RequestedAt = result.RequestedAt,

                DistanceKm = route.DistanceKm,
                DurationMinutes = route.DurationMinutes,

                DurationWithoutTrafficMinutes =
                    route.DurationWithoutTrafficMinutes,

                TrafficAvailable = route.TrafficAvailable,
                TrafficUsed = route.TrafficUsed,

                Error = route.Error
            };

            await routeLogRepository.AddAsync(log, cancellationToken);
        }

        await routeLogRepository.SaveChangesAsync(cancellationToken);

        return result;
    }
}