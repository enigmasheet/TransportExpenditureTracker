using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Helper.ControllerHelpers;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class DriversController(IDriverService driverService, IDriverDataManager driverDataManager, IVehicleDataManager vehicleDataManager, IAuditService audit) : Controller
{
    private const string EntityName = "Driver";
    private const string LogLevelInfo = "Information";
    private static readonly string[] DriverNameRequired = ["Driver name is required."];
    private static readonly string[] DriverNameDuplicate = ["A driver with this name already exists."];
    private static readonly string[] DriverNotFound = ["Driver not found."];

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Drivers", null));
        var drivers = await driverService.GetAllAsync();
        var countMap = await vehicleDataManager.GetDriverVehicleCountsAsync();
        foreach (var d in drivers)
            d.VehicleCount = countMap.GetValueOrDefault(d.DriverId);
        return View(drivers);
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await driverService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_DriverEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickDriverRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.Name))
            return Json(new { success = false, errors = new { name = DriverNameRequired } });
        if (await driverDataManager.ExistsByNameAsync(request.Name))
            return Json(new { success = false, errors = new { name = DriverNameDuplicate } });

        var driver = new Driver
        {
            Name = request.Name,
            LicenseNumber = request.LicenseNumber,
            Phone = request.Phone,
            Address = request.Address,
            HireDate = request.HireDate,
            IsActive = request.IsActive
        };
        await driverDataManager.AddAsync(driver);
        await audit.LogAsync(EntityName, driver.DriverId.ToString(CultureInfo.InvariantCulture), "Create", null, JsonSerializer.Serialize(driver), CurrentUserId, $"Created driver '{driver.Name}'", LogLevelInfo, null);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickDriverUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.Name))
            return Json(new { success = false, errors = new { name = DriverNameRequired } });
        if (await driverDataManager.ExistsByNameAsync(request.Name, request.DriverId))
            return Json(new { success = false, errors = new { name = DriverNameDuplicate } });

        var driver = await driverDataManager.GetByIdAsync(request.DriverId);
        if (driver == null)
            return Json(new { success = false, errors = new { general = DriverNotFound } });

        driver.Name = request.Name;
        driver.LicenseNumber = request.LicenseNumber;
        driver.Phone = request.Phone;
        driver.Address = request.Address;
        driver.HireDate = request.HireDate;
        driver.IsActive = request.IsActive;
        await driverDataManager.UpdateAsync(driver);
        await audit.LogAsync(EntityName, request.DriverId.ToString(CultureInfo.InvariantCulture), "Update", null, JsonSerializer.Serialize(driver), CurrentUserId, $"Updated driver '{driver.Name}'", LogLevelInfo, null);
        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await driverService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        var countMap = await vehicleDataManager.GetDriverVehicleCountsAsync();
        vm.VehicleCount = countMap.GetValueOrDefault(vm.DriverId);
        return PartialView("_DriverDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var driver = await driverDataManager.GetByIdAsync(request.Id);
        if (driver == null)
            return Json(new { success = false, errors = new { general = DriverNotFound } });
        await driverDataManager.DeleteByIdAsync(request.Id);
        await audit.LogAsync(EntityName, request.Id.ToString(CultureInfo.InvariantCulture), "Delete", driver.Name, null, CurrentUserId, $"Deleted driver '{driver.Name}'", "Warning", null);
        return Json(new { success = true });
    }
}