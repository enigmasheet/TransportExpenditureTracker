using System.Text.Json;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Models.ExportJobStatus;

namespace TransportExpenditureTracker.Services;

public partial class ExportBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExportBackgroundJob> _logger;
    private readonly IConfiguration _configuration;

    public ExportBackgroundJob(IServiceScopeFactory scopeFactory, ILogger<ExportBackgroundJob> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var jobService = scope.ServiceProvider.GetRequiredService<IExportJobService>();
            var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();
            var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

            var pendingJobs = await jobService.GetPendingJobsAsync();
            Logs.PendingJobsCount(_logger, pendingJobs.Count);
            foreach (var job in pendingJobs)
            {
                Logs.ProcessingJob(_logger, job.ExportQueueId, job.ReportType, job.Format, job.RecipientEmail);
                await jobService.UpdateStatusAsync(job.ExportQueueId, Processing, null, null);
                try
                {
                    var filters = string.IsNullOrEmpty(job.FilterJson) ? new ReportFilterViewModel() : JsonSerializer.Deserialize<ReportFilterViewModel>(job.FilterJson);
                    var data = await reportService.GetExportDataAsync(job.ReportType, filters ?? new ReportFilterViewModel());

                    byte[] fileBytes;
                    string fileName;
                    if (job.Format == "Excel") { fileBytes = exportService.GenerateExcel(data, job.ReportType); fileName = $"{job.ReportType}_{DateTime.Now:yyyyMMdd}.xlsx"; }
                    else if (job.Format == "CSV") { fileBytes = exportService.GenerateCsv(data); fileName = $"{job.ReportType}_{DateTime.Now:yyyyMMdd}.csv"; }
                    else { fileBytes = exportService.GeneratePdf(data, job.ReportType); fileName = $"{job.ReportType}_{DateTime.Now:yyyyMMdd}.pdf"; }

                    var tempDir = Path.Combine(Path.GetTempPath(), "ExpenseExports");
                    Directory.CreateDirectory(tempDir);
                    var filePath = Path.Combine(tempDir, fileName);
                    await File.WriteAllBytesAsync(filePath, fileBytes, stoppingToken);

                    await jobService.UpdateStatusAsync(job.ExportQueueId, Completed, filePath, null);

                    try
                    {
                        var ccEmail = _configuration["ExportSettings:CcEmail"] ?? "";
                        await emailSender.SendEmailWithAttachmentAsync(
                            toEmail: job.RecipientEmail,
                            ccEmail: ccEmail,
                            subject: $"Your {job.ReportType} Export ({job.Format}) is ready",
                            body: $"Dear user,\n\nPlease find attached your requested {job.Format} export of the {job.ReportType} report.\n\n- Expense Tracker",
                            attachmentBytes: fileBytes,
                            attachmentFileName: fileName
                        );
                        Logs.JobCompleted(_logger, job.ExportQueueId, job.RecipientEmail);
                    }
                    catch (Exception emailEx)
                    {
                        Logs.EmailSendFailed(_logger, job.ExportQueueId, job.RecipientEmail, emailEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    Logs.JobFailed(_logger, job.ExportQueueId, ex.Message, ex);
                    await jobService.UpdateStatusAsync(job.ExportQueueId, Failed, null, ex.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
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

        [LoggerMessage(LogLevel.Error, Message = "Export job {JobId} failed: {Message}")]
        public static partial void JobFailed(ILogger logger, int jobId, string message, Exception exception);
    }
}
