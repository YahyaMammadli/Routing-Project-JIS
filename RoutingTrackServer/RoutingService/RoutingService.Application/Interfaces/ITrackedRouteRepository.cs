using RoutingService.Domain.Entities;

namespace RoutingService.Application.Interfaces;

public interface ITrackedRouteRepository
{
    Task<List<TrackedRoute>> GetActiveAsync(CancellationToken ct = default);
    Task<TrackedRoute?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<TrackedRoute>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TrackedRoute route, CancellationToken ct = default);
    Task RemoveAsync(TrackedRoute route);
    Task AddSnapshotAsync(RouteSnapshot snapshot, CancellationToken ct = default);
    Task<List<RouteSnapshot>> GetSnapshotsAsync(int trackedRouteId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}