using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var currentFiscalYear = now.Month >= 4 ? now.Year.ToString() : (now.Year - 1).ToString();

        var totalExpenditure = await _db.ExpenseDetails.SumAsync(d => d.TotalAmount);
        var totalVatPaid = await _db.ExpenseDetails.SumAsync(d => d.VatAmount);
        var totalTaxableAmount = await _db.ExpenseDetails.SumAsync(d => d.TaxableAmount);
        var totalInvoices = await _db.ExpenseHeaders.CountAsync();

        var thisMonthExpenses = await _db.ExpenseHeaders
            .Where(h => h.EnglishDate.Year == now.Year && h.EnglishDate.Month == now.Month)
            .Join(_db.ExpenseDetails, h => h.ExpenseId, d => d.ExpenseId, (h, d) => d.TotalAmount)
            .SumAsync();

        var thisFiscalYearExpenses = await _db.ExpenseHeaders
            .Where(h => h.FiscalYear == currentFiscalYear)
            .Join(_db.ExpenseDetails, h => h.ExpenseId, d => d.ExpenseId, (h, d) => d.TotalAmount)
            .SumAsync();

        var topSupplierData = await _db.ExpenseHeaders
            .GroupBy(h => h.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.SelectMany(h => h.Details).Sum(d => d.TotalAmount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefaultAsync();

        string? topSupplierName = null;
        if (topSupplierData is not null)
        {
            var supplier = await _db.Suppliers.FindAsync(topSupplierData.SupplierId);
            topSupplierName = supplier?.SupplierName;
        }

        return new DashboardViewModel
        {
            TotalExpenditure = totalExpenditure,
            TotalVatPaid = totalVatPaid,
            TotalTaxableAmount = totalTaxableAmount,
            TotalInvoices = totalInvoices,
            ThisMonthExpenses = thisMonthExpenses,
            ThisFiscalYearExpenses = thisFiscalYearExpenses,
            TopSupplier = topSupplierName
        };
    }
}
