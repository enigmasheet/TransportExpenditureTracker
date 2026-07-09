using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }

    [MaxLength(100)]
    public string EntityName { get; set; } = null!;

    [MaxLength(50)]
    public string EntityId { get; set; } = null!;

    [MaxLength(20)]
    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = null!;
}
