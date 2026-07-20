using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IExpenseDataManager
{
    Task<List<ExpenseHeader>> SearchAsync(string term);
    Task<List<FiscalYear>> GetFiscalYearsAsync();
    Task<List<Supplier>> GetSuppliersAsync();
    Task<List<ExpenseCategory>> GetCategoriesAsync();
    Task<List<Item>> GetItemsAsync();
}
