using Microsoft.EntityFrameworkCore;
using RoutingService.Application.Interfaces;
using RoutingService.Domain.Entities;

namespace RoutingService.Infrastructure.Persistence.Repositories;

public class TrackedRouteRepository : ITrackedRouteRepository
{
    private readonly AppDbContext _context;

    public TrackedRouteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<TrackedRoute>> GetActiveAsync(CancellationToken ct = default)
        => _context.TrackedRoutes
                   .Where(r => r.IsActive)
                   .ToListAsync(ct);

    public Task<TrackedRoute?> GetByIdAsync(int id, CancellationToken ct = default)
        => _context.TrackedRoutes.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<TrackedRoute>> GetAllAsync(CancellationToken ct = default)
        => _context.TrackedRoutes
                   .Include(r => r.Snapshots)
                   .OrderByDescending(r => r.CreatedAt)
                   .ToListAsync(ct);

    public async Task AddAsync(TrackedRoute route, CancellationToken ct = default)
        => await _context.TrackedRoutes.AddAsync(route, ct);

    public Task RemoveAsync(TrackedRoute route)
    {
        _context.TrackedRoutes.Remove(route);
        return Task.CompletedTask;
    }

    public async Task AddSnapshotAsync(RouteSnapshot snapshot, CancellationToken ct = default)
        => await _context.RouteSnapshots.AddAsync(snapshot, ct);

    public Task<List<RouteSnapshot>> GetSnapshotsAsync(int trackedRouteId, CancellationToken ct = default)
        => _context.RouteSnapshots
                   .Where(s => s.TrackedRouteId == trackedRouteId)
                   .OrderBy(s => s.CapturedAt)
                   .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}