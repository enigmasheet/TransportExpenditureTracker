namespace TransportExpenditureTracker.ViewModels;

public class ExportQueueViewModel
{
    public int ExportQueueId { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? FilterJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
