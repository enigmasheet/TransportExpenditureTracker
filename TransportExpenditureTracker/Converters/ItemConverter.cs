using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class ItemConverter
{
    public ItemViewModel ToViewModel(Item i)
    {
        return new ItemViewModel
        {
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            Unit = i.Unit ?? string.Empty
        };
    }

    public Item ToModel(ItemViewModel vm)
    {
        return new Item
        {
            ItemName = vm.ItemName,
            Unit = vm.Unit
        };
    }

    public void UpdateModel(ItemViewModel vm, Item existing)
    {
        existing.ItemName = vm.ItemName;
        existing.Unit = vm.Unit;
    }
}
