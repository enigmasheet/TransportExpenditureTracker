using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IReportExportService
{
    byte[] GenerateExcel(List<ReportRowViewModel> data, string title);
    byte[] GeneratePdf(List<ReportRowViewModel> data, string title);
    byte[] GenerateCsv(List<ReportRowViewModel> data);
}
