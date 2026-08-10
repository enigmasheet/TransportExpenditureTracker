using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class SupplierService(ISupplierDataManager supplierDataManager, SupplierConverter converter) : ISupplierService
{
    public async Task<List<SupplierViewModel>> GetAllAsync()
    {
        var suppliers = await supplierDataManager.GetAllAsync();
        return [.. suppliers.Select(converter.ToViewModel)];
    }

    public async Task<SupplierViewModel?> GetByIdAsync(int id)
    {
        var supplier = await supplierDataManager.GetByIdAsync(id);
        return supplier is null ? null : converter.ToViewModel(supplier);
    }

    public async Task<int> AddAsync(SupplierViewModel vm)
    {
        var model = converter.ToModel(vm);
        await supplierDataManager.AddAsync(model);
        return model.SupplierId;
    }

    public async Task UpdateAsync(SupplierViewModel vm)
    {
        var existing = await supplierDataManager.GetByIdAsync(vm.SupplierId);
        if (existing is null) return;
        SupplierConverter.UpdateModel(vm, existing);
        await supplierDataManager.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        await supplierDataManager.DeleteByIdAsync(id);
    }
}
