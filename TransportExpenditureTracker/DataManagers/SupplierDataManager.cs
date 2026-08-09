using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class SupplierDataManager(ApplicationDbContext db) : ISupplierDataManager
{
    public async Task<List<Supplier>> GetAllAsync()
    {
        return await db.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await db.Suppliers.FindAsync(id);
    }

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = db.Suppliers.AsNoTracking().Where(s => string.Equals(s.SupplierName, name, StringComparison.OrdinalIgnoreCase));
        if (excludeId.HasValue)
            query = query.Where(s => s.SupplierId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        db.Suppliers.Update(supplier);
        await db.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var supplier = await db.Suppliers.FindAsync(id);
        if (supplier is not null)
        {
            db.Suppliers.Remove(supplier);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await db.ExpenseHeaders.AnyAsync(h => h.SupplierId == id);
    }

    public async Task<Dictionary<int, int>> GetHeaderCountsAsync()
    {
        return await db.ExpenseHeaders.AsNoTracking()
            .GroupBy(h => h.SupplierId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}
