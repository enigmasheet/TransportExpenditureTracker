using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

#pragma warning disable CA1711 // Rename type name so that it does not end in 'Queue'
public class ExportQueue
#pragma warning restore CA1711
{
    public int ExportQueueId { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

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
