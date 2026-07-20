using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IReportDataManager
{
    Task<List<FiscalYear>> GetFiscalYearsAsync();
    Task<List<Supplier>> GetSuppliersAsync();
    Task<List<ExpenseCategory>> GetCategoriesAsync();
    Task<List<Item>> GetItemsAsync();
}
