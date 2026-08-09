using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ItemDataManager(ApplicationDbContext db) : IItemDataManager
{
    public async Task<List<Item>> GetAllAsync()
    {
        return await db.Items.OrderBy(i => i.ItemName).ToListAsync();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await db.Items.FindAsync(id);
    }

    public async Task<Item> AddAsync(Item item)
    {
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = db.Items.AsNoTracking().Where(i => string.Equals(i.ItemName, name, StringComparison.OrdinalIgnoreCase));
        if (excludeId.HasValue)
            query = query.Where(i => i.ItemId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(Item item)
    {
        db.Items.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await db.Items.FindAsync(id);
        if (item is not null)
        {
            db.Items.Remove(item);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await db.ExpenseDetails.AnyAsync(d => d.ItemId == id);
    }

    public async Task<Dictionary<int, int>> GetDetailCountsAsync()
    {
        var query = db.ExpenseDetails.AsNoTracking();

        return await query
            .GroupBy(d => d.ItemId)
            .Select(g => new { Id = g.Key, Count = g.Select(d => d.ExpenseId).Distinct().Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}