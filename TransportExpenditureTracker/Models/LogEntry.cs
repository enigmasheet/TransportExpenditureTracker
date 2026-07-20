namespace TransportExpenditureTracker.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Exception { get; set; }

    public string LevelBadge => Level switch
    {
        "ERR" or "FTL" => "danger",
        "WRN" => "warning",
        _ => "info"
    };
}
