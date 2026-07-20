using System.Text.Json;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Models.ExportJobStatus;

namespace TransportExpenditureTracker.Services;

public class ExportBackgroundJob : BackgroundService
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
            _logger.LogInformation("Export check: {Count} pending jobs", pendingJobs.Count);
            foreach (var job in pendingJobs)
            {
                _logger.LogInformation("Processing export job {JobId}: {Type}/{Format} for {Email}",
                    job.ExportQueueId, job.ReportType, job.Format, job.RecipientEmail);
                await jobService.UpdateStatusAsync(job.ExportQueueId, Processing, null, null);
                try
                {
                    var filters = string.IsNullOrEmpty(job.FilterJson) ? new ReportFilterViewModel() : JsonSerializer.Deserialize<ReportFilterViewModel>(job.FilterJson);
                    var data = await reportService.GetDetailedLedgerAsync(filters ?? new ReportFilterViewModel());

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

                    var ccEmail = _configuration["ExportSettings:CcEmail"] ?? "";
                    await emailSender.SendEmailWithAttachmentAsync(
                        toEmail: job.RecipientEmail,
                        ccEmail: ccEmail,
                        subject: $"Your {job.ReportType} Export ({job.Format}) is ready",
                        body: $"Dear user,\n\nPlease find attached your requested {job.Format} export of the {job.ReportType} report.\n\n- Expense Tracker",
                        attachmentBytes: fileBytes,
                        attachmentFileName: fileName
                    );
                    _logger.LogInformation("Export job {JobId} completed, email sent to {Email}",
                        job.ExportQueueId, job.RecipientEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Export job {JobId} failed: {Message}", job.ExportQueueId, ex.Message);
                    await jobService.UpdateStatusAsync(job.ExportQueueId, Failed, null, ex.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
