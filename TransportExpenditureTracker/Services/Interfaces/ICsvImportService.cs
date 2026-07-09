using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface ICsvImportService
{
    Task<CsvPreviewViewModel> PreviewAsync(IFormFile file);
    Task<ImportSummaryViewModel> ImportAsync(CsvPreviewViewModel preview, string userId, bool autoCreate);
}
