using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IVehicleDataManager
{
    Task<List<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetByIdAsync(int id);
    Task<Vehicle> AddAsync(Vehicle vehicle);
    Task<bool> ExistsByNumberAsync(string vehicleNumber, int? excludeId = null);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteByIdAsync(int id);
    Task<List<Driver>> GetActiveDriversAsync();
    Task<Dictionary<int, int>> GetDriverVehicleCountsAsync();
}