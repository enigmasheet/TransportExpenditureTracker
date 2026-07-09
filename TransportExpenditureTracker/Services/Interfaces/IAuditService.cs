using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string userId);
    Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId);
}
