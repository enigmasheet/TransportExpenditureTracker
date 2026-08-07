using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class DashboardService(ApplicationDbContext db, ICurrentUserService currentUser) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardAsync(string? fiscalYear = null)
    {
        var now = DateTime.UtcNow;

        var fiscalYears = await db.FiscalYears
            .OrderByDescending(f => f.Id)
            .Select(f => f.Name)
            .ToListAsync();

        var latestFy = fiscalYears.FirstOrDefault();
        var selectedFy = fiscalYear ?? latestFy;

        var headers = currentUser.IsAdmin
            ? db.ExpenseHeaders
            : db.ExpenseHeaders.Where(h => h.UserId == currentUser.UserId);

        var query = headers.Where(h => h.FiscalYearNav.Name == selectedFy);

        var totalExpenditure = await query
            .Join(db.ExpenseDetails, h => h.ExpenseId, d => d.ExpenseId, (h, d) => d.TotalAmount)
            .SumAsync();
        var totalVatPaid = await query
            .Join(db.ExpenseDetails, h => h.ExpenseId, d => d.ExpenseId, (h, d) => d.VatAmount)
            .SumAsync();
        var totalTaxableAmount = await query
            .Join(db.ExpenseDetails, h => h.ExpenseId, d => d.ExpenseId, (h, d) => d.TaxableAmount)
            .SumAsync();
        var totalInvoices = await query.CountAsync();

        var thisMonthExpenses = await query
            .Where(h => h.EnglishDate.Year == now.Year && h.EnglishDate.Month == now.Month)
            .Join(db.ExpenseDetails, h => h.ExpenseId, d => d.ExpenseId, (h, d) => d.TotalAmount)
            .SumAsync();

        var topSupplierData = await query
            .GroupBy(h => h.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.SelectMany(h => h.Details).Sum(d => d.TotalAmount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefaultAsync();

        string? topSupplierName = null;
        if (topSupplierData is not null)
        {
            var supplier = await db.Suppliers.FindAsync(topSupplierData.SupplierId);
            topSupplierName = supplier?.SupplierName;
        }

        return new DashboardViewModel
        {
            TotalExpenditure = totalExpenditure,
            TotalVatPaid = totalVatPaid,
            TotalTaxableAmount = totalTaxableAmount,
            TotalInvoices = totalInvoices,
            ThisMonthExpenses = thisMonthExpenses,
            ThisFiscalYearExpenses = totalExpenditure,
            TopSupplier = topSupplierName,
            FiscalYears = fiscalYears,
            SelectedFiscalYear = selectedFy
        };
    }
}