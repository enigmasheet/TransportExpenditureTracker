// Services/Interfaces/IReportService.cs
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Services.ReportService;

namespace TransportExpenditureTracker.Services.Interfaces
{
    public interface IReportService
    {
        Task<PagedResult<ReportRowViewModel>> GetVatInvoiceReportAsync(ReportFilterViewModel filters);

    }
}
