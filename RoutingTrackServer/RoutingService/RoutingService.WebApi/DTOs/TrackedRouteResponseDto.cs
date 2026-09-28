namespace RoutingService.WebApi.DTOs;

public class TrackedRouteResponseDto
{
    public int Id { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public int SnapshotCount { get; set; }
}