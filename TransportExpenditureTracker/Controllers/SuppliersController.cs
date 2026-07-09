using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<IActionResult> Index()
    {
        var suppliers = await _supplierService.GetAllAsync();
        return View(suppliers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _supplierService.AddAsync(vm);
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _supplierService.GetByIdAsync(id);
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
            await _supplierService.UpdateAsync(vm);
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _supplierService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var results = await _supplierService.SearchAsync(term);
        return PartialView("_SearchResults", results);
    }

    [HttpGet]
    public async Task<IActionResult> SearchJson(string term)
    {
        var results = await _supplierService.SearchAsync(term);
        var data = results.Select(s => new
        {
            id = s.SupplierId,
            text = $"{s.SupplierName}{(string.IsNullOrEmpty(s.VatNo) ? "" : $" [{s.VatNo}]")}"
        });
        return Json(new { results = data });
    }
}
