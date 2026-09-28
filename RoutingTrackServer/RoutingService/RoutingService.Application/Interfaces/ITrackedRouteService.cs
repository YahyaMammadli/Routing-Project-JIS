using RoutingService.Application.Models;

namespace RoutingService.Application.Interfaces;

public interface ITrackedRouteService
{
    Task<TrackedRouteDto> CreateAsync(string from, string to, CancellationToken ct = default);
    Task<List<TrackedRouteDto>> GetAllAsync(CancellationToken ct = default);
    Task RemoveAsync(int id, CancellationToken ct = default);
    Task<List<RouteSnapshotDto>> GetHistoryAsync(int id, CancellationToken ct = default);
    Task<int> RunAllActiveAsync(CancellationToken ct = default); 
}