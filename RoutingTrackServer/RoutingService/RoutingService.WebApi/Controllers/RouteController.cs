using Microsoft.AspNetCore.Mvc;
using RoutingService.Application.Interfaces;
using RoutingService.Application.Models;
using RoutingService.WebApi.DTOs;

namespace RoutingService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RouteController(IRouteService routeService) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<MultiProviderRouteResult>> Calculate([FromBody] RouteRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.From))
            BadRequest(new {error = "From is required!"});
        

        if (string.IsNullOrWhiteSpace(request.To))
            BadRequest(new {error = "To is required!"});
        

        var applicationRequest =new RouteCalculationRequest(request.From, request.To);

        var result = await routeService.CalculateAndLogAsync(applicationRequest, cancellationToken);

        return Ok(result);
    }

}