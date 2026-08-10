using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class SupplierDataManager(ApplicationDbContext db) : ISupplierDataManager
{
    public async Task<List<Supplier>> GetAllAsync()
    {
        return await db.Suppliers.AsNoTracking().Where(s => !s.IsDeleted).OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<List<Supplier>> GetFuelSuppliersAsync()
    {
        return await db.Suppliers.AsNoTracking()
            .Where(s => !s.IsDeleted && s.IsFuelSupplier)
            .OrderBy(s => s.SupplierName)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await db.Suppliers.AsNoTracking().Where(s => !s.IsDeleted).SingleOrDefaultAsync(s => s.SupplierId == id);
    }

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = db.Suppliers.AsNoTracking().Where(s => !s.IsDeleted && string.Equals(s.SupplierName, name, StringComparison.OrdinalIgnoreCase));
        if (excludeId.HasValue)
            query = query.Where(s => s.SupplierId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        supplier.UpdatedAt = DateTime.Now;
        db.Suppliers.Update(supplier);
        await db.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var supplier = await db.Suppliers.FindAsync(id);
        if (supplier is not null)
        {
            supplier.IsDeleted = true;
            supplier.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<int, int>> GetHeaderCountsAsync()
    {
        return await db.ExpenseHeaders.AsNoTracking()
            .GroupBy(h => h.SupplierId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}
