// Services/Interfaces/IReportService.cs
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<ReportRowViewModel>> GetVatInvoiceReportAsync(ReportFilterViewModel filters);

    }
}
