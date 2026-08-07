using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IReportService
{
    Task<List<ReportRowViewModel>> GetDailyReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetMonthlyReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetFiscalYearReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetSupplierWiseReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetCategoryWiseReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetItemWiseReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetVatPaidReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetPaymentMethodReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetLocationWiseReportAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetDetailedLedgerAsync(ReportFilterViewModel filters);
    Task<List<ReportRowViewModel>> GetExportDataAsync(string reportType, ReportFilterViewModel filters);
    Task<int> GetTotalCountAsync(ReportFilterViewModel filters, string reportType);
}
