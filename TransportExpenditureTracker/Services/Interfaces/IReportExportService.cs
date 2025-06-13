using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces
{
    public interface IReportExportService
    {
        byte[] GenerateVatInvoiceReport(List<ReportRowViewModel> data,Company company);
        byte[] GenerateVatInvoiceExcel(List<ReportRowViewModel> data, Company company);

    }
}
