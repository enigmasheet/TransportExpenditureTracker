using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class DashboardDataManager(ApplicationDbContext db) : IDashboardDataManager
{
    public async Task<List<ChartDataPoint>> GetMonthlyChartDataAsync(string? fiscalYear)
    {
        var query = from h in db.ExpenseHeaders
                    from d in db.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.NepaliMonth into g
                    select new ChartDataPoint(g.Key, g.Sum(d => d.TotalAmount));

        var data = await query.ToListAsync();
        return SortByMonthSuffix(data);
    }

    public async Task<List<ChartDataPoint>> GetCategoryChartDataAsync(string? fiscalYear)
    {
        var query = from h in db.ExpenseHeaders
                    from d in db.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.Category.CategoryName into g
                    select new ChartDataPoint(g.Key, g.Sum(d => d.TotalAmount));

        var data = await query.ToListAsync();
        return [.. data.OrderByDescending(d => d.Value)];
    }

    public async Task<List<ChartDataPoint>> GetSupplierChartDataAsync(string? fiscalYear)
    {
        var query = from h in db.ExpenseHeaders
                    from d in db.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.Supplier.SupplierName into g
                    select new ChartDataPoint(g.Key, g.Sum(d => d.TotalAmount));

        var data = await query.ToListAsync();
        return [.. data.OrderByDescending(d => d.Value).Take(10)];
    }

    public async Task<List<ChartDataPoint>> GetVatChartDataAsync(string? fiscalYear)
    {
        var query = from h in db.ExpenseHeaders
                    from d in db.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.NepaliMonth into g
                    select new ChartDataPoint(g.Key, g.Sum(d => d.VatAmount));

        var data = await query.ToListAsync();
        return SortByMonthSuffix(data);
    }

    public async Task<List<ChartDataPoint>> GetFyComparisonDataAsync(string? fiscalYear)
    {
        var query = from h in db.ExpenseHeaders
                    from d in db.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.FiscalYearNav.Name into g
                    select new ChartDataPoint(g.Key, g.Sum(d => d.TotalAmount));

        var data = await query.ToListAsync();
        return [.. data.Where(d => d.Label != null).OrderBy(d => d.Label)];
    }

    private static List<ChartDataPoint> SortByMonthSuffix(List<ChartDataPoint> data)
    {
        return [.. data
            .Where(d => d.Label != null)
            .Select(d => new
            {
                d.Label,
                d.Value,
                Sort = d.Label!.Contains('(') && d.Label.EndsWith(')')
                    ? int.Parse(d.Label.Split('(', ')', StringSplitOptions.None)[1], System.Globalization.CultureInfo.InvariantCulture)
                    : 0
            })
            .OrderBy(d => d.Sort)
            .Select(d => new ChartDataPoint(d.Label, d.Value))];
    }
}
