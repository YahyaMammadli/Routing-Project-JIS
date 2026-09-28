using RoutingService.Domain.Entities;

namespace TwoGisRouteServer.Application.Interfaces;

public interface IRouteLogRepository
{
    Task AddAsync(RouteLog log, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}