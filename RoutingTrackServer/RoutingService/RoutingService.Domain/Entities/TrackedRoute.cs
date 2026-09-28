namespace RoutingService.Domain.Entities;

public class TrackedRoute
{
    public int Id { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }

    public List<RouteSnapshot> Snapshots { get; set; } = new();
}