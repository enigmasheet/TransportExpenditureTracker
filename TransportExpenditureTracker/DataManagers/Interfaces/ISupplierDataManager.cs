using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface ISupplierDataManager
{
    Task<List<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier> AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task DeleteByIdAsync(int id);
    Task<bool> IsReferencedAsync(int id);
    Task<List<Supplier>> SearchAsync(string? term);
    Task<Dictionary<int, int>> GetHeaderCountsAsync();
}
