using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ExpenseDataManager(ApplicationDbContext db, TransportExpenditureTracker.Services.Interfaces.ICurrentUserService currentUser) : IExpenseDataManager
{
    public async Task<List<ExpenseHeader>> SearchAsync(string term)
    {
        var query = currentUser.IsAdmin
            ? db.ExpenseHeaders.AsNoTracking()
            : db.ExpenseHeaders.AsNoTracking().Where(e => e.UserId == currentUser.UserId);

        return await query
            .Include(e => e.Supplier)
            .Include(e => e.Category)
            .Where(e => e.InvoiceNo.Contains(term)
                || e.Supplier.SupplierName.Contains(term)
                || (e.Remarks != null && e.Remarks.Contains(term)))
            .ToListAsync();
    }

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
