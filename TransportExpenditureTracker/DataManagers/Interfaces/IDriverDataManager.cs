using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IDriverDataManager
{
    Task<List<Driver>> GetAllAsync();
    Task<Driver?> GetByIdAsync(int id);
    Task<Driver> AddAsync(Driver driver);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task UpdateAsync(Driver driver);
    Task DeleteByIdAsync(int id);
}