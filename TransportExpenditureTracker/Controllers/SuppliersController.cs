using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Helper.ControllerHelpers;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class SuppliersController(ISupplierService supplierService, ISupplierDataManager supplierDataManager) : Controller
{
    private static readonly string[] SupplierNameRequired = ["Supplier name is required."];
    private static readonly string[] SupplierNotFound = ["Supplier not found."];
    private static readonly string[] SupplierReferenced = ["Cannot delete: supplier is referenced in existing expense records."];
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
            await supplierService.AddAsync(vm);
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
            await supplierService.UpdateAsync(vm);
            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }
        var inUse = await supplierDataManager.IsReferencedAsync(id);
        if (inUse)
        {
            TempData["Error"] = "Cannot delete: supplier is referenced in existing expense records.";
            return RedirectToAction(nameof(Index));
        }
        await supplierService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var results = await supplierService.SearchAsync(term);
        return PartialView("_SearchResults", results);
    }

    [HttpGet]
    public async Task<IActionResult> SearchJson(string term)
    {
        var results = await supplierService.SearchAsync(term);
        var data = results.Select(s => new
        {
            id = s.SupplierId,
            text = $"{s.SupplierName}{(string.IsNullOrEmpty(s.VatNo) ? "" : $" [{s.VatNo}]")}"
        });
        return Json(new { results = data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickSupplierRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.SupplierName))
            return Json(new { success = false, errors = new { supplierName = SupplierNameRequired } });

        var supplier = new Supplier
        {
            SupplierName = request.SupplierName,
            Location = request.Location,
            VatNo = request.VatNo
        };
        await supplierDataManager.AddAsync(supplier);
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

        var supplier = await supplierDataManager.GetByIdAsync(request.SupplierId);
        if (supplier == null)
            return Json(new { success = false, errors = new { general = SupplierNotFound } });

        supplier.SupplierName = request.SupplierName;
        supplier.Location = request.Location;
        supplier.VatNo = request.VatNo;
        await supplierDataManager.UpdateAsync(supplier);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var vm = await supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_SupplierDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var inUse = await supplierDataManager.IsReferencedAsync(request.Id);
        if (inUse)
            return Json(new { success = false, errors = new { general = SupplierReferenced } });
        await supplierDataManager.DeleteByIdAsync(request.Id);
        return Json(new { success = true });
    }
}
