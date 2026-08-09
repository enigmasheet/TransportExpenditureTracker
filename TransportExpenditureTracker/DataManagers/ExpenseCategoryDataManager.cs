using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ExpenseCategoryDataManager(ApplicationDbContext db) : IExpenseCategoryDataManager
{
    public async Task<List<ExpenseCategory>> GetAllAsync()
    {
        return await db.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
    }

    public async Task<ExpenseCategory?> GetByIdAsync(int id)
    {
        return await db.ExpenseCategories.FindAsync(id);
    }

    public async Task<ExpenseCategory> AddAsync(ExpenseCategory category)
    {
        db.ExpenseCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = db.ExpenseCategories.AsNoTracking().Where(c => string.Equals(c.CategoryName, name, StringComparison.OrdinalIgnoreCase));
        if (excludeId.HasValue)
            query = query.Where(c => c.CategoryId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(ExpenseCategory category)
    {
        db.ExpenseCategories.Update(category);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await db.ExpenseCategories.FindAsync(id);
        if (category is not null)
        {
            db.ExpenseCategories.Remove(category);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await db.ExpenseHeaders.AnyAsync(h => h.CategoryId == id);
    }

    public async Task<Dictionary<int, int>> GetHeaderCountsAsync()
    {
        var query = db.ExpenseHeaders.AsNoTracking();

        return await query
            .GroupBy(h => h.CategoryId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}
