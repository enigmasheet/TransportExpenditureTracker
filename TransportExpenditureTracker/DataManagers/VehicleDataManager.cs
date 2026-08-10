using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.DataManagers;

public class VehicleDataManager(ApplicationDbContext db) : IVehicleDataManager
{
    public async Task<List<Vehicle>> GetAllAsync()
    {
        return await db.Vehicles.AsNoTracking()
            .Include(v => v.AssignedDriver)
            .Where(v => !v.IsDeleted)
            .OrderBy(v => v.VehicleNumber)
            .ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await db.Vehicles.AsNoTracking()
            .Include(v => v.AssignedDriver)
            .Where(v => !v.IsDeleted)
            .SingleOrDefaultAsync(v => v.VehicleId == id);
    }

    public async Task<Vehicle> AddAsync(Vehicle vehicle)
    {
        vehicle.CreatedAt = DateTime.Now;
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        return vehicle;
    }

    public async Task<bool> ExistsByNumberAsync(string vehicleNumber, int? excludeId = null)
    {
        var query = db.Vehicles.AsNoTracking().Where(v => !v.IsDeleted && string.Equals(v.VehicleNumber, vehicleNumber, StringComparison.OrdinalIgnoreCase));
        if (excludeId.HasValue)
            query = query.Where(v => v.VehicleId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        vehicle.UpdatedAt = DateTime.Now;
        db.Vehicles.Update(vehicle);
        await db.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var vehicle = await db.Vehicles.FindAsync(id);
        if (vehicle is not null)
        {
            vehicle.IsDeleted = true;
            vehicle.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<Driver>> GetActiveDriversAsync()
    {
        return await db.Drivers.AsNoTracking()
            .Where(d => !d.IsDeleted && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<int, int>> GetDriverVehicleCountsAsync()
    {
        return await db.Vehicles.AsNoTracking()
            .Where(v => !v.IsDeleted && v.AssignedDriverId.HasValue)
            .GroupBy(v => v.AssignedDriverId!.Value)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Id, g => g.Count);
    }
}