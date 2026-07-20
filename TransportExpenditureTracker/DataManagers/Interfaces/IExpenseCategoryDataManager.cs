using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers.Interfaces;

public interface IExpenseCategoryDataManager
{
    Task<List<ExpenseCategory>> GetAllAsync();
    Task<ExpenseCategory?> GetByIdAsync(int id);
    Task<ExpenseCategory> AddAsync(ExpenseCategory category);
    Task UpdateAsync(ExpenseCategory category);
    Task DeleteAsync(int id);
    Task<bool> IsReferencedAsync(int id);
    Task<Dictionary<int, int>> GetHeaderCountsAsync();
}
