using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ExportsController : Controller
{
    private readonly IExportJobService _exportJobService;
    private readonly ApplicationDbContext _ctx;

    public ExportsController(IExportJobService exportJobService, ApplicationDbContext ctx)
    {
        _exportJobService = exportJobService;
        _ctx = ctx;
    }

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Exports", null));
        var jobs = await _exportJobService.GetAllAsync();
        return View(jobs);
    }

    public async Task<IActionResult> Details(int id)
    {
        var allJobs = await _exportJobService.GetAllAsync();
        var job = allJobs.FirstOrDefault(j => j.ExportQueueId == id);
        if (job == null) return NotFound();
        return View(job);
    }

    public async Task<IActionResult> Download(int id)
    {
        var allJobs = await _exportJobService.GetAllAsync();
        var job = allJobs.FirstOrDefault(j => j.ExportQueueId == id);
        if (job == null || job.Status != "Completed" || string.IsNullOrEmpty(job.FilePath))
            return NotFound();

        if (!System.IO.File.Exists(job.FilePath))
            return NotFound("File no longer available.");

        var ext = Path.GetExtension(job.FilePath)?.ToLowerInvariant();
        var contentType = ext switch
        {
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var fileName = Path.GetFileName(job.FilePath);
        return PhysicalFile(job.FilePath, contentType, fileName);
    }

    public async Task<IActionResult> GetStatus()
    {
        var jobs = await _exportJobService.GetAllAsync();
        return PartialView("_ExportTable", jobs);
    }
}