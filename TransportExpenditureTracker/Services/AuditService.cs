using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Models.Pagination;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class AuditService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager) : IAuditService
{
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
            UserId = userId,
            RemoteIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        };

        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, string userId, string? message, string? logLevel, string? remoteIp)
    {
        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Message = message,
            LogLevel = logLevel ?? "Information",
            RemoteIp = remoteIp ?? httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        };

        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId)
    {
        return await db.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<PagedResult<AuditLogViewModel>> GetAllAsync(AuditLogFilter filter)
    {
        var query = db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(a => a.EntityName == filter.EntityName);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (!string.IsNullOrWhiteSpace(filter.LogLevel))
            query = query.Where(a => a.LogLevel == filter.LogLevel);

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.ToLower(CultureInfo.InvariantCulture);
            query = query.Where(a =>
                a.EntityName.ToLower(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                a.Message!.ToLower(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                a.Action.ToLower(CultureInfo.InvariantCulture).Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = (filter.SortColumn?.ToLower(CultureInfo.InvariantCulture), filter.SortDirection?.ToLower(CultureInfo.InvariantCulture)) switch
        {
            ("entityname", "asc") => query.OrderBy(a => a.EntityName),
            ("entityname", _) => query.OrderByDescending(a => a.EntityName),
            ("action", "asc") => query.OrderBy(a => a.Action),
            ("action", _) => query.OrderByDescending(a => a.Action),
            ("loglevel", "asc") => query.OrderBy(a => a.LogLevel),
            ("loglevel", _) => query.OrderByDescending(a => a.LogLevel),
            ("user", "asc") => query.OrderBy(a => a.UserId),
            ("user", _) => query.OrderByDescending(a => a.UserId),
            _ => query.OrderByDescending(a => a.Timestamp)
        };

        var paged = await query.ToPagedResultAsync(filter.Page, filter.PageSize);

        var userIds = paged.Items.Select(i => i.UserId).Distinct().ToList();
        var users = await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "Unknown");

        var viewModels = paged.Items.Select(a => new AuditLogViewModel
        {
            AuditLogId = a.AuditLogId,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            Action = a.Action,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            Timestamp = a.Timestamp,
            UserId = a.UserId,
            UserName = users.GetValueOrDefault(a.UserId, "Unknown"),
            LogLevel = a.LogLevel,
            Message = a.Message,
            RemoteIp = a.RemoteIp
        }).ToList();

        return new PagedResult<AuditLogViewModel>(viewModels, paged.PageNumber, paged.PageSize, paged.TotalCount);
    }

    public async Task<List<string>> GetDistinctEntityNamesAsync()
    {
        return await db.AuditLogs
            .AsNoTracking()
            .Select(a => a.EntityName)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
    }

    public async Task<AuditLogViewModel?> GetByIdAsync(int id)
    {
        var log = await db.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.AuditLogId == id);
        if (log is null) return null;

        var user = await userManager.FindByIdAsync(log.UserId);
        var userName = user?.UserName ?? user?.Email ?? "Unknown";

        return new AuditLogViewModel
        {
            AuditLogId = log.AuditLogId,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Action = log.Action,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            Timestamp = log.Timestamp,
            UserId = log.UserId,
            UserName = userName,
            LogLevel = log.LogLevel,
            Message = log.Message,
            RemoteIp = log.RemoteIp
        };
    }
}
