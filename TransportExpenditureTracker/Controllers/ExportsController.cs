using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportExpenditureTracker.Data;
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
}
