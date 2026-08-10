using System.Text.Json;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Models.ExportJobStatus;

namespace TransportExpenditureTracker.Services;

public partial class ExportBackgroundJob(IServiceScopeFactory scopeFactory, ILogger<ExportBackgroundJob> logger, IConfiguration configuration) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // A transient DB/IO error must never stop the background service or kill the host.
                Logs.PollFailed(logger, ex.Message, ex);
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PollOnceAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<IExportJobService>();
        var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

        var pendingJobs = await jobService.GetPendingJobsAsync();
        Logs.PendingJobsCount(logger, pendingJobs.Count);
        foreach (var job in pendingJobs)
        {
            Logs.ProcessingJob(logger, job.ExportQueueId, job.ReportType, job.Format, job.RecipientEmail);
            await jobService.UpdateStatusAsync(job.ExportQueueId, Processing, null, null);
            try
            {
                var filters = string.IsNullOrEmpty(job.FilterJson) ? new ReportFilterViewModel() : JsonSerializer.Deserialize<ReportFilterViewModel>(job.FilterJson);
                var data = await reportService.GetExportDataAsync(job.ReportType, filters ?? new ReportFilterViewModel());

                byte[] fileBytes;
                string fileName;
                if (job.Format == "Excel") { fileBytes = exportService.GenerateExcel(data, job.ReportType); fileName = $"{job.ReportType}_{job.ExportQueueId}_{DateTime.Now:yyyyMMdd}.xlsx"; }
                else if (job.Format == "CSV") { fileBytes = exportService.GenerateCsv(data); fileName = $"{job.ReportType}_{job.ExportQueueId}_{DateTime.Now:yyyyMMdd}.csv"; }
                else { fileBytes = exportService.GeneratePdf(data, job.ReportType); fileName = $"{job.ReportType}_{job.ExportQueueId}_{DateTime.Now:yyyyMMdd}.pdf"; }

                var tempDir = Path.Combine(Path.GetTempPath(), "ExpenseExports");
                Directory.CreateDirectory(tempDir);
                var filePath = Path.Combine(tempDir, fileName);
                await File.WriteAllBytesAsync(filePath, fileBytes, stoppingToken);

                CleanupStaleFiles(tempDir);

                if (!emailSender.IsConfigured)
                {
                    var message = "Export file was generated but the email was not sent because SMTP is not configured (Resend:ApiKey is missing).";
                    await jobService.UpdateStatusAsync(job.ExportQueueId, Completed, filePath, message);
                    Logs.EmailNotConfigured(logger, job.ExportQueueId, job.RecipientEmail);
                    continue;
                }

                try
                {
                    var ccEmail = configuration["ExportSettings:CcEmail"] ?? "";
                    await emailSender.SendEmailWithAttachmentAsync(
                        toEmail: job.RecipientEmail,
                        ccEmail: ccEmail,
                        subject: $"Your {job.ReportType} Export ({job.Format}) is ready",
                        body: $"Dear user,\n\nPlease find attached your requested {job.Format} export of the {job.ReportType} report.\n\n- Expense Tracker",
                        attachmentBytes: fileBytes,
                        attachmentFileName: fileName
                    );
                    await jobService.UpdateStatusAsync(job.ExportQueueId, Completed, filePath, null, DateTime.UtcNow);
                    Logs.JobCompleted(logger, job.ExportQueueId, job.RecipientEmail);
                }
                catch (Exception emailEx)
                {
                    await jobService.UpdateStatusAsync(job.ExportQueueId, Completed, filePath, $"Email send failed: {emailEx.Message}");
                    Logs.EmailSendFailed(logger, job.ExportQueueId, job.RecipientEmail, emailEx.Message);
                }
            }
            catch (Exception ex)
            {
                Logs.JobFailed(logger, job.ExportQueueId, ex.Message, ex);
                await jobService.UpdateStatusAsync(job.ExportQueueId, Failed, null, ex.Message);
            }
        }
    }

    private void CleanupStaleFiles(string tempDir)
    {
        try
        {
            var cutoff = DateTime.UtcNow - RetentionPeriod;
            var deleted = 0;
            foreach (var file in Directory.GetFiles(tempDir, "*.*"))
            {
                if (File.GetLastWriteTimeUtc(file) < cutoff)
                {
                    try
                    {
                        File.Delete(file);
                        deleted++;
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            if (deleted > 0)
                Logs.StaleFilesRemoved(logger, deleted);
        }
        catch (Exception ex)
        {
            Logs.StaleFilesCleanupFailed(logger, ex.Message, ex);
        }
    }

    private static partial class Logs
    {
        [LoggerMessage(LogLevel.Information, Message = "Export check: {Count} pending jobs")]
        public static partial void PendingJobsCount(ILogger logger, int count);

        [LoggerMessage(LogLevel.Information, Message = "Processing export job {JobId}: {Type}/{Format} for {Email}")]
        public static partial void ProcessingJob(ILogger logger, int jobId, string type, string format, string email);

        [LoggerMessage(LogLevel.Information, Message = "Export job {JobId} completed, email sent to {Email}")]
        public static partial void JobCompleted(ILogger logger, int jobId, string email);

        [LoggerMessage(LogLevel.Warning, Message = "Export job {JobId} completed but email to {Email} failed: {Message}")]
        public static partial void EmailSendFailed(ILogger logger, int jobId, string email, string message);

        [LoggerMessage(LogLevel.Warning, Message = "Export job {JobId} completed but email to {Email} was not sent (SMTP not configured)")]
        public static partial void EmailNotConfigured(ILogger logger, int jobId, string email);

        [LoggerMessage(LogLevel.Error, Message = "Export job {JobId} failed: {Message}")]
        public static partial void JobFailed(ILogger logger, int jobId, string message, Exception exception);

        [LoggerMessage(LogLevel.Error, Message = "Export polling pass failed: {Message}")]
        public static partial void PollFailed(ILogger logger, string message, Exception exception);

        [LoggerMessage(LogLevel.Information, Message = "Cleaned up {Count} stale export file(s)")]
        public static partial void StaleFilesRemoved(ILogger logger, int count);

        [LoggerMessage(LogLevel.Warning, Message = "Failed to clean up stale export files: {Message}")]
        public static partial void StaleFilesCleanupFailed(ILogger logger, string message, Exception exception);
    }
}