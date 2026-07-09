using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _db;
    private readonly SupplierConverter _converter;

    public SupplierService(ApplicationDbContext db, SupplierConverter converter)
    {
        _db = db;
        _converter = converter;
    }

    public async Task<List<SupplierViewModel>> GetAllAsync()
    {
        var suppliers = await _db.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
        return suppliers.Select(_converter.ToViewModel).ToList();
    }

    public async Task<SupplierViewModel?> GetByIdAsync(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        return supplier is null ? null : _converter.ToViewModel(supplier);
    }

    public async Task AddAsync(SupplierViewModel vm)
    {
        var model = _converter.ToModel(vm);
        _db.Suppliers.Add(model);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SupplierViewModel vm)
    {
        var existing = await _db.Suppliers.FindAsync(vm.SupplierId);
        if (existing is null) return;
        _converter.UpdateModel(vm, existing);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null) return;
        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync();
    }

    public async Task<List<SupplierViewModel>> SearchAsync(string term)
    {
        var query = _db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(s => s.SupplierName.Contains(term) || (s.VatNo != null && s.VatNo.Contains(term)));
        }
        var results = await query.OrderBy(s => s.SupplierName).ToListAsync();
        return results.Select(_converter.ToViewModel).ToList();
    }
}
