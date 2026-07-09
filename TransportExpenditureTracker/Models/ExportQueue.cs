using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class ExportQueue
{
    public int ExportQueueId { get; set; }

    public DateTime RequestedAt { get; set; }

    [MaxLength(10)]
    public string Format { get; set; } = null!;

    [MaxLength(50)]
    public string ReportType { get; set; } = null!;

    public string? FilterJson { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = null!;

    [MaxLength(500)]
    public string? FilePath { get; set; }

    [MaxLength(200)]
    public string RecipientEmail { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }
}
