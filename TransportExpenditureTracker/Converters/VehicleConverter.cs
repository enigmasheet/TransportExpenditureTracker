using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class VehicleConverter
{
    public VehicleViewModel ToViewModel(Vehicle v)
    {
        return new VehicleViewModel
        {
            VehicleId = v.VehicleId,
            VehicleNumber = v.VehicleNumber,
            Type = v.Type,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            EngineNumber = v.EngineNumber,
            ChassisNumber = v.ChassisNumber,
            CurrentMeterReading = v.CurrentMeterReading,
            AssignedDriverId = v.AssignedDriverId,
            DriverName = v.AssignedDriver?.Name,
            IsActive = v.IsActive
        };
    }

    public Vehicle ToModel(VehicleViewModel vm)
    {
        return new Vehicle
        {
            VehicleNumber = vm.VehicleNumber,
            Type = vm.Type,
            Make = vm.Make,
            Model = vm.Model,
            Year = vm.Year,
            EngineNumber = vm.EngineNumber,
            ChassisNumber = vm.ChassisNumber,
            CurrentMeterReading = vm.CurrentMeterReading,
            AssignedDriverId = vm.AssignedDriverId,
            IsActive = vm.IsActive
        };
    }

    public static void UpdateModel(VehicleViewModel vm, Vehicle existing)
    {
        existing.VehicleNumber = vm.VehicleNumber;
        existing.Type = vm.Type;
        existing.Make = vm.Make;
        existing.Model = vm.Model;
        existing.Year = vm.Year;
        existing.EngineNumber = vm.EngineNumber;
        existing.ChassisNumber = vm.ChassisNumber;
        existing.CurrentMeterReading = vm.CurrentMeterReading;
        existing.AssignedDriverId = vm.AssignedDriverId;
        existing.IsActive = vm.IsActive;
    }
}