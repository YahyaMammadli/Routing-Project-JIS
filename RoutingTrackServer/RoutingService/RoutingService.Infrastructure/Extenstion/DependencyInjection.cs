using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoutingService.Application.Interfaces;
using RoutingService.Infrastructure.Persistence;
using RoutingService.Infrastructure.Persistence.Repositories;
using RoutingService.Infrastructure.RoutingProviders.TwoGis;
using RoutingService.Infrastructure.RoutingProviders.Yandex;
using TwoGisRouteServer.Application.Interfaces;
using TwoGisRouteServer.Infrastructure.Persistence.Repositories;

namespace RoutingService.Infrastructure.Extenstion;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRouteLogRepository, RouteLogRepository>();

        services.AddScoped<ITrackedRouteRepository, TrackedRouteRepository>();

        services.Configure<TwoGisOptions>(configuration.GetSection("RoutingProviders:TwoGis"));

        services.Configure<YandexOptions>(configuration.GetSection("RoutingProviders:Yandex"));

        //services.Configure<GoogleOptions>(configuration.GetSection("RoutingProviders:Google"));

        services.AddHttpClient<TwoGisRoutingProvider>();

        services.AddHttpClient<YandexRoutingProvider>();

        //services.AddHttpClient<GoogleRoutingProvider>();

        services.AddScoped<IRoutingProvider, TwoGisRoutingProvider>();

        services.AddScoped<IRoutingProvider, YandexRoutingProvider>();

        //services.AddScoped<IRoutingProvider, GoogleRoutingProvider>();

        return services;
    }
}