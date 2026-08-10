using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class DriverConverter
{
    public DriverViewModel ToViewModel(Driver d)
    {
        return new DriverViewModel
        {
            DriverId = d.DriverId,
            Name = d.Name,
            LicenseNumber = d.LicenseNumber,
            Phone = d.Phone,
            Address = d.Address,
            HireDate = d.HireDate,
            IsActive = d.IsActive
        };
    }

    public Driver ToModel(DriverViewModel vm)
    {
        return new Driver
        {
            Name = vm.Name,
            LicenseNumber = vm.LicenseNumber,
            Phone = vm.Phone,
            Address = vm.Address,
            HireDate = vm.HireDate,
            IsActive = vm.IsActive
        };
    }

    public static void UpdateModel(DriverViewModel vm, Driver existing)
    {
        existing.Name = vm.Name;
        existing.LicenseNumber = vm.LicenseNumber;
        existing.Phone = vm.Phone;
        existing.Address = vm.Address;
        existing.HireDate = vm.HireDate;
        existing.IsActive = vm.IsActive;
    }
}