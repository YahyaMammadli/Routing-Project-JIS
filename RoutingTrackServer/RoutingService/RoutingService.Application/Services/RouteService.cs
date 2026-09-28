using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Entities;

namespace RoutingService.Application.Services;

public class RouteService(
    IRoutingAggregatorService routingAggregatorService,
    IRouteLogRepository routeLogRepository) : IRouteService
{
    public async Task<MultiProviderRouteResult> CalculateAndLogAsync(
        RouteCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await routingAggregatorService.CalculateAsync(
            request,
            cancellationToken);

        foreach (var providerRoute in result.Routes)
        {
            if (providerRoute.Routes.Count == 0)
            {
                // Preserve provider errors in the log even when no route was found.
                await routeLogRepository.AddAsync(
                    new RouteLog
                    {
                        FromAddress = result.From,
                        ToAddress = result.To,
                        Provider = providerRoute.Provider,
                        TransportMode = providerRoute.TransportMode,
                        RequestedAt = result.RequestedAt,
                        DistanceKm = providerRoute.DistanceKm,
                        DurationMinutes = providerRoute.DurationMinutes,
                        DurationWithoutTrafficMinutes = providerRoute.DurationWithoutTrafficMinutes,
                        TrafficAvailable = providerRoute.TrafficAvailable,
                        TrafficUsed = providerRoute.TrafficUsed,
                        Error = providerRoute.Error
                    },
                    cancellationToken);

                continue;
            }

            // A provider can return several alternatives (2GIS).
            // Store each returned alternative as a separate log record.
            foreach (var route in providerRoute.Routes)
            {
                await routeLogRepository.AddAsync(
                    new RouteLog
                    {
                        FromAddress = result.From,
                        ToAddress = result.To,
                        Provider = providerRoute.Provider,
                        TransportMode = providerRoute.TransportMode,
                        RequestedAt = result.RequestedAt,
                        DistanceKm = route.DistanceKm,
                        DurationMinutes = route.DurationMinutes,
                        DurationWithoutTrafficMinutes =
                            route.DurationWithoutTrafficMinutes ??
                            providerRoute.DurationWithoutTrafficMinutes,
                        TrafficAvailable = providerRoute.TrafficAvailable,
                        TrafficUsed = providerRoute.TrafficUsed,
                        Error = null
                    },
                    cancellationToken);
            }
        }

        await routeLogRepository.SaveChangesAsync(cancellationToken);

        return result;
    }
}
