using RoutingService.Application.Models;
using RoutingService.WebApi.DTOs;
using TwoGisRouteServer.API.DTOs;
using TwoGisRouteServer.Application.Models;

namespace TwoGisRouteServer.API.Mappings;

public static class RouteMappingExtensions
{
    public static RouteCalculationRequest ToApplicationModel(this RouteRequestDto dto)
        => new RouteCalculationRequest(dto.From, dto.To);

    public static RouteInfoResponseDto ToResponseDto(this RouteCalculationResult result)
        => new RouteInfoResponseDto
        {
            CurrentTime = result.CurrentTime,
            DurationInTrafficMinutes = result.DurationInTrafficMinutes,
            DurationWithoutTrafficMinutes = result.DurationWithoutTrafficMinutes,
            TrafficLevel = result.TrafficLevel,
            AverageTaxiPrice = result.AverageTaxiPrice,
            DistanceKm = result.DistanceKm
        };
}