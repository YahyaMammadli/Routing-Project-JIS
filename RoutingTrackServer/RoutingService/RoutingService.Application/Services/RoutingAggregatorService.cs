using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.Domain.Enums;

namespace RoutingService.Application.Services;

public class RoutingAggregatorService(IEnumerable<IRoutingProvider> providers) : IRoutingAggregatorService
{



    public async Task<MultiProviderRouteResult> CalculateAsync(RouteCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var modes = Enum.GetValues<TransportMode>();

        var tasks = providers
            .SelectMany(provider =>
                modes.Select(mode =>
                    CalculateProviderAsync(
                        provider,
                        request,
                        mode,
                        cancellationToken)))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return new MultiProviderRouteResult
        {
            From = request.From,
            To = request.To,
            RequestedAt = DateTime.UtcNow,
            Routes = results.ToList()
        };
    }





    private static async Task<ProviderRouteResult> CalculateProviderAsync(IRoutingProvider provider,RouteCalculationRequest request,TransportMode mode,CancellationToken cancellationToken)
    {
        try
        {
            return await provider.CalculateAsync(
                request,
                mode,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return new ProviderRouteResult
            {
                Provider = provider.Name,
                TransportMode = mode.ToString(),
                Error = ex.Message
            };
        }
    }



}