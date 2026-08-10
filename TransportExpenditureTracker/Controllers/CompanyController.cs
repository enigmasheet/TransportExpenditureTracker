using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class CompanyController(ICompanyService companyService, IAuditService audit) : Controller
{
    private const string CtlDashboard = "Dashboard";
    private const string HomeLabel = "Home";

    public async Task<IActionResult> Setup()
    {
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), ("Company Setup", null));

        var company = await companyService.GetAsync();
        var model = company ?? new Company { DefaultVatRate = AppConstants.VatRate };
        ViewBag.IsNew = company is null;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(Company model)
    {
        if (!ModelState.IsValid)
        {
            var existing = await companyService.GetAsync();
            ViewBag.IsNew = existing is null;
            return View(model);
        }

        var isNew = await companyService.IsSetupCompleteAsync() == false;

        if (!isNew && !(User.IsInRole("Admin") || User.IsInRole("SuperAdmin")))
        {
            TempData["Error"] = "Only admins can modify company settings after the initial setup.";
            return RedirectToAction(nameof(Index), "Dashboard");
        }

        var before = isNew ? null : JsonSerializer.Serialize(await companyService.GetAsync());
        var saved = await companyService.SaveAsync(model);

        await audit.LogAsync(
            "Company",
            saved.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            isNew ? "Create" : "Update",
            before,
            JsonSerializer.Serialize(saved),
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            $"Company setup {(isNew ? "completed" : "updated")}: '{saved.Name}'",
            "Info",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "Company information saved successfully.";
        return RedirectToAction(nameof(Index), "Dashboard");
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Setup));
    }
}