using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Models.ExportJobStatus;

namespace TransportExpenditureTracker.Services;

public class ExportJobService(ApplicationDbContext db, TransportExpenditureTracker.Services.Interfaces.ICurrentUserService currentUser) : IExportJobService
{
    public async Task<int> EnqueueAsync(string format, string reportType, string? filterJson, string recipientEmail, string userId)
    {
        var job = new ExportQueue
        {
            Format = format,
            ReportType = reportType,
            FilterJson = filterJson,
            RecipientEmail = recipientEmail,
            UserId = userId,
            Status = Pending,
            RequestedAt = DateTime.UtcNow
        };

        db.ExportQueues.Add(job);
        await db.SaveChangesAsync();

        return job.ExportQueueId;
    }

    public async Task<List<ExportQueueViewModel>> GetPendingJobsAsync()
    {
        return await db.ExportQueues
            .Where(j => j.Status == Pending)
            .OrderBy(j => j.RequestedAt)
            .Select(j => new ExportQueueViewModel
            {
                ExportQueueId = j.ExportQueueId,
                RequestedAt = j.RequestedAt,
                Format = j.Format,
                ReportType = j.ReportType,
                Status = j.Status,
                RecipientEmail = j.RecipientEmail,
                FilePath = j.FilePath,
                SentAt = j.SentAt,
                ErrorMessage = j.ErrorMessage
            })
            .ToListAsync();
    }

    public async Task<List<ExportQueueViewModel>> GetAllAsync()
    {
        var query = db.ExportQueues.AsNoTracking();
        if (!currentUser.IsAdmin)
            query = query.Where(j => j.UserId == currentUser.UserId);

        return await query
            .OrderByDescending(j => j.RequestedAt)
            .Select(j => new ExportQueueViewModel
            {
                ExportQueueId = j.ExportQueueId,
                RequestedAt = j.RequestedAt,
                Format = j.Format,
                ReportType = j.ReportType,
                Status = j.Status,
                RecipientEmail = j.RecipientEmail,
                FilePath = j.FilePath,
                SentAt = j.SentAt,
                ErrorMessage = j.ErrorMessage
            })
            .ToListAsync();
    }

public async Task UpdateStatusAsync(int id, string status, string? filePath, string? errorMessage)
    {
        var job = await db.ExportQueues.FindAsync(id);
        if (job is null) return;

        job.Status = status;
        if (filePath is not null)
            job.FilePath = filePath;
        if (errorMessage is not null)
            job.ErrorMessage = errorMessage;
        if (status == Completed)
            job.SentAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }
}
