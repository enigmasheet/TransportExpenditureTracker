using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierViewModel>> GetAllAsync();
    Task<SupplierViewModel?> GetByIdAsync(int id);
    Task AddAsync(SupplierViewModel vm);
    Task UpdateAsync(SupplierViewModel vm);
    Task DeleteAsync(int id);
}
