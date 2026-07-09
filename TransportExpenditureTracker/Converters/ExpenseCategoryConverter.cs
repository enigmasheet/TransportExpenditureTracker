using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class ExpenseCategoryConverter
{
    public ExpenseCategoryViewModel ToViewModel(ExpenseCategory c)
    {
        return new ExpenseCategoryViewModel
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName
        };
    }

    public ExpenseCategory ToModel(ExpenseCategoryViewModel vm)
    {
        return new ExpenseCategory
        {
            CategoryName = vm.CategoryName
        };
    }

    public void UpdateModel(ExpenseCategoryViewModel vm, ExpenseCategory existing)
    {
        existing.CategoryName = vm.CategoryName;
    }
}
