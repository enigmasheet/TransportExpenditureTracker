using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IExportJobService
{
    Task<int> EnqueueAsync(string format, string reportType, string? filterJson, string recipientEmail);
    Task<List<ExportQueueViewModel>> GetPendingJobsAsync();
    Task<List<ExportQueueViewModel>> GetAllAsync();
    Task UpdateStatusAsync(int id, string status, string? filePath, string? errorMessage);
}
