using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class SupplierConverter
{
    public SupplierViewModel ToViewModel(Supplier s)
    {
        return new SupplierViewModel
        {
            SupplierId = s.SupplierId,
            SupplierName = s.SupplierName,
            Location = s.Location,
            VatNo = s.VatNo,
            IsFuelSupplier = s.IsFuelSupplier,
            IsActive = s.IsActive
        };
    }

    public Supplier ToModel(SupplierViewModel vm)
    {
        return new Supplier
        {
            SupplierName = vm.SupplierName,
            Location = vm.Location,
            VatNo = vm.VatNo,
            IsFuelSupplier = vm.IsFuelSupplier,
            IsActive = vm.IsActive
        };
    }

    public static void UpdateModel(SupplierViewModel vm, Supplier existing)
    {
        existing.SupplierName = vm.SupplierName;
        existing.Location = vm.Location;
        existing.VatNo = vm.VatNo;
        existing.IsFuelSupplier = vm.IsFuelSupplier;
        existing.IsActive = vm.IsActive;
    }
}
