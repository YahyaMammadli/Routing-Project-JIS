using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoutingService.Application.Interfaces;

namespace RoutingService.Infrastructure.BackgroundJobs;

public class RouteTrackingJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RouteTrackingJob> _logger;

    public RouteTrackingJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RouteTrackingJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RouteTrackingJob started. Interval: {Interval}", Interval);

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RouteTrackingJob iteration failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("RouteTrackingJob stopped");
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITrackedRouteService>();

        _logger.LogInformation("Running periodic route tracking...");
        var count = await service.RunAllActiveAsync(ct);
        _logger.LogInformation("Tracked {Count} route(s)", count);
    }
}