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
        return await db.Suppliers.AsNoTracking().Where(s => !s.IsDeleted).OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
    {
        return await db.PaymentMethods.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .ToListAsync();
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
