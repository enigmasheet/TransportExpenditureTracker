using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class VehicleService(IVehicleDataManager vehicleDataManager, VehicleConverter converter) : IVehicleService
{
    public async Task<List<VehicleViewModel>> GetAllAsync()
    {
        var vehicles = await vehicleDataManager.GetAllAsync();
        return [.. vehicles.Select(converter.ToViewModel)];
    }

    public async Task<VehicleViewModel?> GetByIdAsync(int id)
    {
        var vehicle = await vehicleDataManager.GetByIdAsync(id);
        return vehicle is null ? null : converter.ToViewModel(vehicle);
    }

    public async Task AddAsync(VehicleViewModel vm)
    {
        var model = converter.ToModel(vm);
        await vehicleDataManager.AddAsync(model);
    }

    public async Task UpdateAsync(VehicleViewModel vm)
    {
        var existing = await vehicleDataManager.GetByIdAsync(vm.VehicleId);
        if (existing is null) return;
        VehicleConverter.UpdateModel(vm, existing);
        await vehicleDataManager.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        await vehicleDataManager.DeleteByIdAsync(id);
    }
}