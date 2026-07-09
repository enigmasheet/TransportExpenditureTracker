using System.Text.Json;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class ExportBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExportBackgroundJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var jobService = scope.ServiceProvider.GetRequiredService<IExportJobService>();
            var exportService = scope.ServiceProvider.GetRequiredService<IReportExportService>();
            var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();

            var pendingJobs = await jobService.GetPendingJobsAsync();
            foreach (var job in pendingJobs)
            {
                await jobService.UpdateStatusAsync(job.ExportQueueId, "Processing", null, null);
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

                    await jobService.UpdateStatusAsync(job.ExportQueueId, "Completed", filePath, null);
                }
                catch (Exception ex)
                {
                    await jobService.UpdateStatusAsync(job.ExportQueueId, "Failed", null, ex.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
