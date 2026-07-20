using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ExpenseDataManager : IExpenseDataManager
{
    private readonly ApplicationDbContext _db;

    public ExpenseDataManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExpenseHeader>> SearchAsync(string term)
    {
        return await _db.ExpenseHeaders
            .Include(e => e.Supplier)
            .Include(e => e.Category)
            .Where(e => e.InvoiceNo.Contains(term)
                || e.Supplier.SupplierName.Contains(term)
                || (e.Remarks != null && e.Remarks.Contains(term)))
            .ToListAsync();
    }

    public async Task<List<FiscalYear>> GetFiscalYearsAsync()
    {
        return await _db.FiscalYears.OrderByDescending(f => f.Id).ToListAsync();
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await _db.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
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
