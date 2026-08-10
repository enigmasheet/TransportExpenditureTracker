using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class DriverDataManager(ApplicationDbContext db) : IDriverDataManager
{
    public async Task<List<Driver>> GetAllAsync()
    {
        return await db.Drivers.AsNoTracking()
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Driver?> GetByIdAsync(int id)
    {
        return await db.Drivers.AsNoTracking()
            .Where(d => !d.IsDeleted)
            .SingleOrDefaultAsync(d => d.DriverId == id);
    }

    public async Task<Driver> AddAsync(Driver driver)
    {
        driver.CreatedAt = DateTime.Now;
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
        return driver;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = db.Drivers.AsNoTracking().Where(d => !d.IsDeleted && string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        if (excludeId.HasValue)
            query = query.Where(d => d.DriverId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(Driver driver)
    {
        driver.UpdatedAt = DateTime.Now;
        db.Drivers.Update(driver);
        await db.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var driver = await db.Drivers.FindAsync(id);
        if (driver is not null)
        {
            driver.IsDeleted = true;
            driver.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }
}