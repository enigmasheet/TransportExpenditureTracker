using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class ItemDataManager : IItemDataManager
{
    private readonly ApplicationDbContext _db;

    public ItemDataManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        return await _db.Items.OrderBy(i => i.ItemName).ToListAsync();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _db.Items.FindAsync(id);
    }

    public async Task<Item> AddAsync(Item item)
    {
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(Item item)
    {
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item is not null)
        {
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await _db.ExpenseDetails.AnyAsync(d => d.ItemId == id);
    }

    public async Task<Dictionary<int, int>> GetDetailCountsAsync()
    {
        return await _db.ExpenseDetails
            .GroupBy(d => d.ItemId)
            .Select(g => new { Id = g.Key, Count = g.Select(d => d.ExpenseId).Distinct().Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}
