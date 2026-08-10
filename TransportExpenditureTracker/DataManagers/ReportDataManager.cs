using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ReportDataManager : IReportDataManager
{
    private readonly ApplicationDbContext _db;

    public ReportDataManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<FiscalYear>> GetFiscalYearsAsync()
    {
        return await _db.FiscalYears.OrderByDescending(f => f.Id).ToListAsync();
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await _db.Suppliers.Where(s => !s.IsDeleted).OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        return await _db.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        return await _db.Items.OrderBy(i => i.ItemName).ToListAsync();
    }
}
