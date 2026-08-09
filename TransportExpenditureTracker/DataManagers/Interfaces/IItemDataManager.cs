using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IItemDataManager
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<Item> AddAsync(Item item);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
    Task<bool> IsReferencedAsync(int id);
    Task<Dictionary<int, int>> GetDetailCountsAsync();
}
