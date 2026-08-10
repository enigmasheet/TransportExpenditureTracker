using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleViewModel>> GetAllAsync();
    Task<VehicleViewModel?> GetByIdAsync(int id);
    Task AddAsync(VehicleViewModel vm);
    Task UpdateAsync(VehicleViewModel vm);
    Task DeleteAsync(int id);
}