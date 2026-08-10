using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IDriverService
{
    Task<List<DriverViewModel>> GetAllAsync();
    Task<DriverViewModel?> GetByIdAsync(int id);
    Task AddAsync(DriverViewModel vm);
    Task UpdateAsync(DriverViewModel vm);
    Task DeleteAsync(int id);
}