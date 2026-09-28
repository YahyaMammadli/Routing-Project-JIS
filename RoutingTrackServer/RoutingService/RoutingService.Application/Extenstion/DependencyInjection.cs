using Microsoft.Extensions.DependencyInjection;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Services;

namespace RoutingService.Application.Extenstion;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoutingAggregatorService, RoutingAggregatorService>();

        services.AddScoped<IRouteService, RouteService>();

        services.AddScoped<ITrackedRouteService, TrackedRouteService>();

        return services;
    }
}