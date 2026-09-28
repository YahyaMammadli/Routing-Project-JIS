using Microsoft.AspNetCore.Mvc;
using RoutingService.Application.Interfaces;
using RoutingService.WebApi.DTOs;

namespace RoutingService.WebApi.Controllers;

[ApiController]
[Route("api/tracked-routes")]
public class TrackedRoutesController : ControllerBase
{
    private readonly ITrackedRouteService _service;

    public TrackedRoutesController(ITrackedRouteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _service.GetAllAsync(ct);
        return Ok(items.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTrackedRouteDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.From) || string.IsNullOrWhiteSpace(dto.To))
            return BadRequest(new { error = "Both 'from' and 'to' are required." });

        var created = await _service.CreateAsync(dto.From, dto.To, ct);
        return Ok(ToResponse(created));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.RemoveAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id, CancellationToken ct)
    {
        var history = await _service.GetHistoryAsync(id, ct);
        return Ok(history.Select(s => new RouteSnapshotResponseDto
        {
            CapturedAt = s.CapturedAt,
            DurationInTrafficMinutes = s.DurationInTrafficMinutes,
            DurationWithoutTrafficMinutes = (double)s.DurationWithoutTrafficMinutes,
            TrafficLevel = s.TrafficLevel,
            AverageTaxiPrice = s.AverageTaxiPrice,
            DistanceKm = s.DistanceKm
        }));
    }

    [HttpPost("run-now")]
    public async Task<IActionResult> RunNow(CancellationToken ct)
    {
        var count = await _service.RunAllActiveAsync(ct);
        return Ok(new { saved = count });
    }

    private static TrackedRouteResponseDto ToResponse(Application.Models.TrackedRouteDto dto) => new()
    {
        Id = dto.Id,
        FromAddress = dto.FromAddress,
        ToAddress = dto.ToAddress,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        LastCheckedAt = dto.LastCheckedAt,
        SnapshotCount = dto.SnapshotCount
    };
}