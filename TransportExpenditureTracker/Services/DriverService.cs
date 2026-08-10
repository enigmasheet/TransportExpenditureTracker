using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class DriverService(IDriverDataManager driverDataManager, DriverConverter converter) : IDriverService
{
    public async Task<List<DriverViewModel>> GetAllAsync()
    {
        var drivers = await driverDataManager.GetAllAsync();
        return [.. drivers.Select(converter.ToViewModel)];
    }

    public async Task<DriverViewModel?> GetByIdAsync(int id)
    {
        var driver = await driverDataManager.GetByIdAsync(id);
        return driver is null ? null : converter.ToViewModel(driver);
    }

    public async Task AddAsync(DriverViewModel vm)
    {
        var model = converter.ToModel(vm);
        await driverDataManager.AddAsync(model);
    }

    public async Task UpdateAsync(DriverViewModel vm)
    {
        var existing = await driverDataManager.GetByIdAsync(vm.DriverId);
        if (existing is null) return;
        DriverConverter.UpdateModel(vm, existing);
        await driverDataManager.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        await driverDataManager.DeleteByIdAsync(id);
    }
}