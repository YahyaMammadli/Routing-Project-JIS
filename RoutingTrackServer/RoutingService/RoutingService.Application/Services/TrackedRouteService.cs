using Microsoft.Extensions.Logging;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Entities;
using TwoGisRouteServer.Application.Models;

namespace RoutingService.Application.Services;

public class TrackedRouteService(
    ITrackedRouteRepository repo,
    IRoutingAggregatorService routingAggregator,
    ILogger<TrackedRouteService> logger) : ITrackedRouteService
{
    public async Task<TrackedRouteDto> CreateAsync(string from, string to, CancellationToken ct = default)
    {
        var entity = new TrackedRoute
        {
            FromAddress = from,
            ToAddress = to,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);

        return Map(entity, 0);
    }

    public async Task<List<TrackedRouteDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var all = await repo.GetAllAsync(ct);

        return all
            .Select(r => Map(r, r.Snapshots?.Count ?? 0))
            .ToList();
    }




    public async Task RemoveAsync(
        int id,
        CancellationToken ct = default)
    {
        var entity = await repo.GetByIdAsync(id, ct);

        if (entity is null)
            return;

        await repo.RemoveAsync(entity);
        await repo.SaveChangesAsync(ct);
    }



    public async Task<List<RouteSnapshotDto>> GetHistoryAsync(
        int id,
        CancellationToken ct = default)
    {
        var snapshots = await repo.GetSnapshotsAsync(id, ct);

        return snapshots
            .Select(s => new RouteSnapshotDto
            {
                CapturedAt = s.CapturedAt,
                DurationInTrafficMinutes = s.DurationInTrafficMinutes,
                DurationWithoutTrafficMinutes =
                    s.DurationWithoutTrafficMinutes,
                TrafficLevel = s.TrafficLevel,
                AverageTaxiPrice = s.AverageTaxiPrice,
                DistanceKm = s.DistanceKm
            })
            .ToList();
    }




    public async Task<int> RunAllActiveAsync(CancellationToken ct = default)
    {
        var active = await repo.GetActiveAsync(ct);

        int saved = 0;

        foreach (var tracked in active)
        {
            try
            {
                var request = new RouteCalculationRequest(
                    tracked.FromAddress,
                    tracked.ToAddress);

                var result =
                    await routingAggregator.CalculateAsync(
                        request,
                        ct);


                tracked.LastCheckedAt = DateTime.UtcNow;

                await repo.SaveChangesAsync(ct);

                saved++;

                logger.LogInformation(
                    "Route #{Id} checked: {From} → {To}",
                    tracked.Id,
                    tracked.FromAddress,
                    tracked.ToAddress);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to track route #{Id} ({From} → {To})",
                    tracked.Id,
                    tracked.FromAddress,
                    tracked.ToAddress);
            }
        }

        return saved;
    }




    private static TrackedRouteDto Map(TrackedRoute route,int snapshotCount)
        =>  new TrackedRouteDto
        {
            Id = route.Id,
            FromAddress = route.FromAddress,
            ToAddress = route.ToAddress,
            IsActive = route.IsActive,
            CreatedAt = route.CreatedAt,
            LastCheckedAt = route.LastCheckedAt,
            SnapshotCount = snapshotCount
        };
    
}