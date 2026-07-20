namespace TransportExpenditureTracker.Models;

public class AuditLogFilter
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public string? LogLevel { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchText { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
}
