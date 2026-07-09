using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class ExportJobService : IExportJobService
{
    private readonly ApplicationDbContext _db;

    public ExportJobService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> EnqueueAsync(string format, string reportType, string? filterJson, string recipientEmail)
    {
        var job = new ExportQueue
        {
            Format = format,
            ReportType = reportType,
            FilterJson = filterJson,
            RecipientEmail = recipientEmail,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _db.ExportQueues.Add(job);
        await _db.SaveChangesAsync();

        return job.ExportQueueId;
    }

    public async Task<List<ExportQueueViewModel>> GetPendingJobsAsync()
    {
        return await _db.ExportQueues
            .Where(j => j.Status == "Pending")
            .OrderBy(j => j.RequestedAt)
            .Select(j => new ExportQueueViewModel
            {
                ExportQueueId = j.ExportQueueId,
                RequestedAt = j.RequestedAt,
                Format = j.Format,
                ReportType = j.ReportType,
                Status = j.Status,
                RecipientEmail = j.RecipientEmail,
                SentAt = j.SentAt,
                ErrorMessage = j.ErrorMessage
            })
            .ToListAsync();
    }

    public async Task<List<ExportQueueViewModel>> GetAllAsync()
    {
        return await _db.ExportQueues
            .OrderByDescending(j => j.RequestedAt)
            .Select(j => new ExportQueueViewModel
            {
                ExportQueueId = j.ExportQueueId,
                RequestedAt = j.RequestedAt,
                Format = j.Format,
                ReportType = j.ReportType,
                Status = j.Status,
                RecipientEmail = j.RecipientEmail,
                SentAt = j.SentAt,
                ErrorMessage = j.ErrorMessage
            })
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, string status, string? filePath, string? error)
    {
        var job = await _db.ExportQueues.FindAsync(id);
        if (job is null) return;

        job.Status = status;
        if (filePath is not null)
            job.FilePath = filePath;
        if (error is not null)
            job.ErrorMessage = error;
        if (status == "Completed")
            job.SentAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
