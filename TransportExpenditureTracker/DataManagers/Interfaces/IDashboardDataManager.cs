using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IDashboardDataManager
{
    Task<List<ChartDataPoint>> GetMonthlyChartDataAsync(string? fiscalYear);
    Task<List<ChartDataPoint>> GetCategoryChartDataAsync(string? fiscalYear);
    Task<List<ChartDataPoint>> GetSupplierChartDataAsync(string? fiscalYear);
    Task<List<ChartDataPoint>> GetVatChartDataAsync(string? fiscalYear);
    Task<List<ChartDataPoint>> GetFyComparisonDataAsync(string? fiscalYear);
}
