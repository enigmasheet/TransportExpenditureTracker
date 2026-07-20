using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ExpenseCategoryDataManager : IExpenseCategoryDataManager
{
    private readonly ApplicationDbContext _db;

    public ExpenseCategoryDataManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExpenseCategory>> GetAllAsync()
    {
        return await _db.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
    }

    public async Task<ExpenseCategory?> GetByIdAsync(int id)
    {
        return await _db.ExpenseCategories.FindAsync(id);
    }

    public async Task<ExpenseCategory> AddAsync(ExpenseCategory category)
    {
        _db.ExpenseCategories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(ExpenseCategory category)
    {
        _db.ExpenseCategories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _db.ExpenseCategories.FindAsync(id);
        if (category is not null)
        {
            _db.ExpenseCategories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await _db.ExpenseHeaders.AnyAsync(h => h.CategoryId == id);
    }

    public async Task<Dictionary<int, int>> GetHeaderCountsAsync()
    {
        return await _db.ExpenseHeaders
            .GroupBy(h => h.CategoryId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}
