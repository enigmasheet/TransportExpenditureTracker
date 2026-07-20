using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public partial class LogsController : Controller
{
    private static readonly string LogFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TransportExpenditureTracker", "logs");

    private static readonly Regex LogLineRegex = LogLinePattern();

    [GeneratedRegex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[(\w{3})\] (.+)$", RegexOptions.Compiled)]
    private static partial Regex LogLinePattern();

    public IActionResult Index(string? date, string? level)
    {
        this.SetBreadcrumbs(
            ("Home", Url.Action("Index", "Dashboard")),
            ("Logs", null));

        ViewBag.AvailableDates = GetLogFilesOnDisk();
        ViewBag.SelectedDate = date ?? GetTodayFileName();
        ViewBag.SelectedLevel = level ?? "";

        return View();
    }

    [HttpGet]
    public IActionResult GetData(string? date, string? level, string? search)
    {
        var entries = ParseLogFile(date ?? GetTodayFileName());

        if (!string.IsNullOrWhiteSpace(level))
            entries = entries.Where(e => e.Level == level).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower(CultureInfo.InvariantCulture);
            entries = entries.Where(e =>
                e.Message.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                (e.Exception?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        return Json(new
        {
            recordsTotal = entries.Count,
            recordsFiltered = entries.Count,
            data = entries.Select(e => new
            {
                timestamp = e.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                e.Level,
                levelBadge = e.LevelBadge,
                e.Message,
                e.Exception
            })
        });
    }

    [HttpGet]
    public IActionResult GetAvailableDates()
    {
        return Json(GetLogFilesOnDisk());
    }

    private static List<string> GetLogFilesOnDisk()
    {
        if (!Directory.Exists(LogFolder))
            return [GetTodayFileName()];

        var files = Directory.GetFiles(LogFolder, "app-*.log")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(static f => f is not null)
            .Select(static f => f!)
            .OrderByDescending(static f => f)
            .ToList();

        return files.Count > 0 ? files : [GetTodayFileName()];
    }

    private static string GetTodayFileName()
    {
        return $"app-{DateTime.UtcNow:yyyy-MM-dd}";
    }

    private static List<LogEntry> ParseLogFile(string fileName)
    {
        var filePath = Path.Combine(LogFolder, $"{fileName}.log");
        if (!System.IO.File.Exists(filePath))
            return [];

        var lines = System.IO.File.ReadAllLines(filePath);
        var entries = new List<LogEntry>();
        LogEntry? current = null;

        foreach (var line in lines)
        {
            var match = LogLineRegex.Match(line);
            if (match.Success)
            {
                if (current != null)
                    entries.Add(current);

                current = new LogEntry
                {
                    Timestamp = DateTime.ParseExact(match.Groups[1].Value, "yyyy-MM-dd HH:mm:ss.fff",
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                    Level = match.Groups[2].Value,
                    Message = match.Groups[3].Value
                };
            }
            else if (current != null)
            {
                current.Exception = current.Exception == null
                    ? line
                    : current.Exception + Environment.NewLine + line;
            }
        }

        if (current != null)
            entries.Add(current);

        return entries;
    }
}
