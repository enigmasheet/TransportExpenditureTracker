using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
public class VehiclesController(IVehicleService vehicleService, IVehicleDataManager vehicleDataManager, IAuditService audit) : Controller
{
    private const string EntityName = "Vehicle";
    private const string LogLevelInfo = "Information";
    private static readonly string[] VehicleNumberRequired = ["Vehicle number is required."];
    private static readonly string[] VehicleNumberDuplicate = ["A vehicle with this number already exists."];
    private static readonly string[] VehicleNotFound = ["Vehicle not found."];

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Vehicles", null));
        var vehicles = await vehicleService.GetAllAsync();
        await LoadDriverOptionsAsync();
        return View(vehicles);
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await vehicleService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        await LoadDriverOptionsAsync(vm.AssignedDriverId);
        return PartialView("_VehicleEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickVehicleRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.VehicleNumber))
            return Json(new { success = false, errors = new { vehicleNumber = VehicleNumberRequired } });
        if (await vehicleDataManager.ExistsByNumberAsync(request.VehicleNumber))
            return Json(new { success = false, errors = new { vehicleNumber = VehicleNumberDuplicate } });

        var vehicle = new Vehicle
        {
            VehicleNumber = request.VehicleNumber,
            Type = request.Type,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            EngineNumber = request.EngineNumber,
            ChassisNumber = request.ChassisNumber,
            CurrentMeterReading = request.CurrentMeterReading,
            AssignedDriverId = request.AssignedDriverId,
            IsActive = request.IsActive
        };
        await vehicleDataManager.AddAsync(vehicle);
        await audit.LogAsync(EntityName, vehicle.VehicleId.ToString(CultureInfo.InvariantCulture), "Create", null, JsonSerializer.Serialize(vehicle), CurrentUserId, $"Created vehicle '{vehicle.VehicleNumber}'", LogLevelInfo, null);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickVehicleUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.VehicleNumber))
            return Json(new { success = false, errors = new { vehicleNumber = VehicleNumberRequired } });
        if (await vehicleDataManager.ExistsByNumberAsync(request.VehicleNumber, request.VehicleId))
            return Json(new { success = false, errors = new { vehicleNumber = VehicleNumberDuplicate } });

        var vehicle = await vehicleDataManager.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
            return Json(new { success = false, errors = new { general = VehicleNotFound } });

        vehicle.VehicleNumber = request.VehicleNumber;
        vehicle.Type = request.Type;
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.EngineNumber = request.EngineNumber;
        vehicle.ChassisNumber = request.ChassisNumber;
        vehicle.CurrentMeterReading = request.CurrentMeterReading;
        vehicle.AssignedDriverId = request.AssignedDriverId;
        vehicle.IsActive = request.IsActive;
        await vehicleDataManager.UpdateAsync(vehicle);
        await audit.LogAsync(EntityName, request.VehicleId.ToString(CultureInfo.InvariantCulture), "Update", null, JsonSerializer.Serialize(vehicle), CurrentUserId, $"Updated vehicle '{vehicle.VehicleNumber}'", LogLevelInfo, null);
        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await vehicleService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_VehicleDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var vehicle = await vehicleDataManager.GetByIdAsync(request.Id);
        if (vehicle == null)
            return Json(new { success = false, errors = new { general = VehicleNotFound } });
        await vehicleDataManager.DeleteByIdAsync(request.Id);
        await audit.LogAsync(EntityName, request.Id.ToString(CultureInfo.InvariantCulture), "Delete", vehicle.VehicleNumber, null, CurrentUserId, $"Deleted vehicle '{vehicle.VehicleNumber}'", "Warning", null);
        return Json(new { success = true });
    }

    private async Task LoadDriverOptionsAsync(int? selected = null)
    {
        var drivers = await vehicleDataManager.GetActiveDriversAsync();
        ViewBag.Drivers = new SelectList(drivers, "DriverId", "Name", selected);
    }
}