using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Models.Pagination;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string userId);

    Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string userId, string? message, string? logLevel, string? remoteIp);

    Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId);

    Task<PagedResult<AuditLogViewModel>> GetAllAsync(AuditLogFilter filter);

    Task<AuditLogViewModel?> GetByIdAsync(int id);

    Task<List<string>> GetDistinctEntityNamesAsync();
}
