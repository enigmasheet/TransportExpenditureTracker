namespace TransportExpenditureTracker.ViewModels;

public class AuditLogViewModel
{
    public int AuditLogId { get; set; }
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = null!;
    public string? UserName { get; set; }
    public string? LogLevel { get; set; }
    public string? Message { get; set; }
    public string? RemoteIp { get; set; }

    public string LogLevelBadge => LogLevel switch
    {
        "Error" => "danger",
        "Warning" => "warning",
        _ => "info"
    };
}
