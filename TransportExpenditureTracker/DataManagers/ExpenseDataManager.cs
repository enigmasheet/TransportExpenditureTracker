using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ExpenseDataManager(ApplicationDbContext db) : IExpenseDataManager
{
    public async Task<List<FiscalYear>> GetFiscalYearsAsync()
    {
        return await db.FiscalYears.OrderByDescending(f => f.Id).ToListAsync();
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await db.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        return await db.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        return await db.Items.OrderBy(i => i.ItemName).ToListAsync();
    }
}
