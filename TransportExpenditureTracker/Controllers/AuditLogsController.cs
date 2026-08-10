using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Policy = "RequireSuperAdminRole")]
public class AuditLogsController(IAuditService auditService) : Controller
{
    public async Task<IActionResult> Index(AuditLogFilter filter)
    {
        this.SetBreadcrumbs(
            ("Home", Url.Action("Index", "Dashboard")),
            ("Audit Logs", null));

        var entityNames = await auditService.GetDistinctEntityNamesAsync();
        ViewBag.EntityNames = entityNames;

        return View(filter);
    }

    [HttpGet]
    public async Task<IActionResult> GetData(AuditLogFilter filter)
    {
        var result = await auditService.GetAllAsync(filter);

        return Json(new
        {
            recordsTotal = result.TotalCount,
            recordsFiltered = result.TotalCount,
            data = result.Items.Select(a => new
            {
                a.AuditLogId,
                timestamp = a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                a.UserName,
                a.EntityName,
                a.Action,
                a.Message,
                logLevel = a.LogLevel ?? "Information",
                logLevelBadge = a.LogLevelBadge,
                a.UserId
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var vm = await auditService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_DetailModal", vm);
    }

    [HttpGet]
    public async Task<IActionResult> Export(AuditLogFilter filter)
    {
        filter.PageSize = int.MaxValue;
        filter.Page = 1;
        var result = await auditService.GetAllAsync(filter);

        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,User,Entity,EntityId,Action,LogLevel,Message,OldValues,NewValues,RemoteIp");

        foreach (var a in result.Items)
        {
            var timestamp = a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var userName = a.UserName ?? "";
            var entityName = a.EntityName;
            var entityId = a.EntityId;
            var action = a.Action;
            var logLevel = a.LogLevel ?? "";
            var message = EscapeCsv(a.Message ?? "");
            var oldValues = EscapeCsv(a.OldValues ?? "");
            var newValues = EscapeCsv(a.NewValues ?? "");
            var remoteIp = EscapeCsv(a.RemoteIp ?? "");
            csv.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                timestamp, userName, entityName, entityId, action, logLevel, message, oldValues, newValues, remoteIp));
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"audit-log-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> GetEntityNames()
    {
        var names = await auditService.GetDistinctEntityNamesAsync();
        return Json(names);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
