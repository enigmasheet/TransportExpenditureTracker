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
public class SuppliersController(ISupplierService supplierService, ISupplierDataManager supplierDataManager, IAuditService audit) : Controller
{
    private const string EntityName = "Supplier";
    private const string LogLevelInfo = "Information";
    private static readonly string[] SupplierNameRequired = ["Supplier name is required."];
    private static readonly string[] SupplierNameDuplicate = ["A supplier with this name already exists."];
    private static readonly string[] SupplierNotFound = ["Supplier not found."];
    private static readonly string[] SupplierReferenced = ["Cannot delete: supplier is referenced in existing expense records."];

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Suppliers", null));
        var suppliers = await supplierService.GetAllAsync();
        var countMap = await supplierDataManager.GetHeaderCountsAsync();
        foreach (var s in suppliers)
            s.ExpenseCount = countMap.GetValueOrDefault(s.SupplierId);
        return View(suppliers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] SupplierViewModel vm)
    {
        if (ModelState.IsValid)
        {
            if (await supplierDataManager.ExistsByNameAsync(vm.SupplierName))
            {
                return Json(new { success = false, errors = new { supplierName = SupplierNameDuplicate } });
            }
            await supplierService.AddAsync(vm);
            await audit.LogAsync(EntityName, "0", "Create", null, JsonSerializer.Serialize(vm), CurrentUserId, $"Created supplier '{vm.SupplierName}'", LogLevelInfo, null);
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierViewModel vm)
    {
        if (id != vm.SupplierId) return NotFound();
        if (ModelState.IsValid)
        {
            if (await supplierDataManager.ExistsByNameAsync(vm.SupplierName, vm.SupplierId))
            {
                ModelState.AddModelError(nameof(vm.SupplierName), "A supplier with this name already exists.");
            }
            else
            {
                await supplierService.UpdateAsync(vm);
                await audit.LogAsync(EntityName, id.ToString(CultureInfo.InvariantCulture), "Update", null, JsonSerializer.Serialize(vm), CurrentUserId, $"Updated supplier '{vm.SupplierName}'", LogLevelInfo, null);
                return RedirectToAction(nameof(Index));
            }
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        return await Edit(id);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }
        var supplier = await supplierDataManager.GetByIdAsync(id);
        if (supplier == null)
        {
            TempData["Error"] = "Supplier not found.";
            return RedirectToAction(nameof(Index));
        }
        var inUse = await supplierDataManager.IsReferencedAsync(id);
        if (inUse)
        {
            TempData["Error"] = "Cannot delete: supplier is referenced in existing expense records.";
            return RedirectToAction(nameof(Index));
        }
        await supplierService.DeleteAsync(id);
        await audit.LogAsync(EntityName, id.ToString(CultureInfo.InvariantCulture), "Delete", supplier.SupplierName, null, CurrentUserId, $"Deleted supplier '{supplier.SupplierName}'", "Warning", null);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickSupplierRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.SupplierName))
            return Json(new { success = false, errors = new { supplierName = SupplierNameRequired } });
        if (await supplierDataManager.ExistsByNameAsync(request.SupplierName))
            return Json(new { success = false, errors = new { supplierName = SupplierNameDuplicate } });

        var supplier = new Supplier
        {
            SupplierName = request.SupplierName,
            Location = request.Location,
            VatNo = request.VatNo
        };
        await supplierDataManager.AddAsync(supplier);
        await audit.LogAsync(EntityName, supplier.SupplierId.ToString(CultureInfo.InvariantCulture), "Create", null, JsonSerializer.Serialize(supplier), CurrentUserId, $"Quick-created supplier '{supplier.SupplierName}'", LogLevelInfo, null);
        return Json(new { success = true, id = supplier.SupplierId, text = supplier.SupplierName });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_SupplierEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickSupplierUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.SupplierName))
            return Json(new { success = false, errors = new { supplierName = SupplierNameRequired } });
        if (await supplierDataManager.ExistsByNameAsync(request.SupplierName, request.SupplierId))
            return Json(new { success = false, errors = new { supplierName = SupplierNameDuplicate } });

        var supplier = await supplierDataManager.GetByIdAsync(request.SupplierId);
        if (supplier == null)
            return Json(new { success = false, errors = new { general = SupplierNotFound } });

        supplier.SupplierName = request.SupplierName;
        supplier.Location = request.Location;
        supplier.VatNo = request.VatNo;
        await supplierDataManager.UpdateAsync(supplier);
        await audit.LogAsync(EntityName, request.SupplierId.ToString(CultureInfo.InvariantCulture), "Update", null, JsonSerializer.Serialize(supplier), CurrentUserId, $"Updated supplier '{supplier.SupplierName}'", LogLevelInfo, null);
        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_SupplierDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var supplier = await supplierDataManager.GetByIdAsync(request.Id);
        if (supplier == null)
            return Json(new { success = false, errors = new { general = SupplierNotFound } });
        var inUse = await supplierDataManager.IsReferencedAsync(request.Id);
        if (inUse)
            return Json(new { success = false, errors = new { general = SupplierReferenced } });
        await supplierDataManager.DeleteByIdAsync(request.Id);
        await audit.LogAsync(EntityName, request.Id.ToString(CultureInfo.InvariantCulture), "Delete", supplier.SupplierName, null, CurrentUserId, $"Quick-deleted supplier '{supplier.SupplierName}'", "Warning", null);
        return Json(new { success = true });
    }
}
