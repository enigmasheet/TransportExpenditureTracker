using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class SupplierDataManager : ISupplierDataManager
{
    private readonly ApplicationDbContext _db;

    public SupplierDataManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Supplier>> GetAllAsync()
    {
        return await _db.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _db.Suppliers.FindAsync(id);
    }

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is not null)
        {
            _db.Suppliers.Remove(supplier);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await _db.ExpenseHeaders.AnyAsync(h => h.SupplierId == id);
    }

    public async Task<List<Supplier>> SearchAsync(string? term)
    {
        var query = _db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(s => s.SupplierName.Contains(term) || (s.VatNo != null && s.VatNo.Contains(term)));
        }
        return await query.OrderBy(s => s.SupplierName).ToListAsync();
    }

    public async Task<Dictionary<int, int>> GetHeaderCountsAsync()
    {
        return await _db.ExpenseHeaders
            .GroupBy(h => h.SupplierId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}
