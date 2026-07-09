using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string userId)
    {
        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = DateTime.UtcNow,
            UserId = userId
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId)
    {
        return await _db.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
