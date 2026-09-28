using RoutingService.Domain.Entities;
using RoutingService.Infrastructure.Persistence;
using TwoGisRouteServer.Application.Interfaces;

namespace TwoGisRouteServer.Infrastructure.Persistence.Repositories;

public class RouteLogRepository : IRouteLogRepository
{
    private readonly AppDbContext _context;

    public RouteLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RouteLog log, CancellationToken cancellationToken = default)
    {
        await _context.RouteLogs.AddAsync(log, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}