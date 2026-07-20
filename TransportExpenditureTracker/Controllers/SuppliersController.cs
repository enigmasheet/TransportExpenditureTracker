using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ISupplierService _supplierService;
    private readonly ApplicationDbContext _ctx;

    public SuppliersController(ISupplierService supplierService, ApplicationDbContext ctx)
    {
        _supplierService = supplierService;
        _ctx = ctx;
    }

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Suppliers", null));
        var suppliers = await _supplierService.GetAllAsync();
        var headerData = await _ctx.ExpenseHeaders
            .Select(h => h.SupplierId)
            .ToListAsync();
        var counts = headerData
            .GroupBy(id => id)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToList();
        var countMap = counts.ToDictionary(c => c.Id, c => c.Count);
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
            await _supplierService.AddAsync(vm);
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickSupplierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierName))
            return Json(new { success = false, errors = new { supplierName = new[] { "Supplier name is required." } } });

        var supplier = new Supplier
        {
            SupplierName = request.SupplierName,
            Location = request.Location,
            VatNo = request.VatNo
        };
        _ctx.Suppliers.Add(supplier);
        await _ctx.SaveChangesAsync();
        return Json(new { success = true, id = supplier.SupplierId, text = supplier.SupplierName });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var vm = await _supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_SupplierEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickSupplierUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierName))
            return Json(new { success = false, errors = new { supplierName = new[] { "Supplier name is required." } } });

        var supplier = await _ctx.Suppliers.FindAsync(request.SupplierId);
        if (supplier == null)
            return Json(new { success = false, errors = new { general = new[] { "Supplier not found." } } });

        supplier.SupplierName = request.SupplierName;
        supplier.Location = request.Location;
        supplier.VatNo = request.VatNo;
        await _ctx.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        var vm = await _supplierService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_SupplierDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        await _supplierService.DeleteAsync(request.Id);
        return Json(new { success = true });
    }

    private static Dictionary<string, string[]> GetModelStateErrors(ModelStateDictionary modelState)
    {
        return modelState
            .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
            .ToDictionary(
                kv => char.ToLowerInvariant(kv.Key[0]) + kv.Key.Substring(1),
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }

    public class QuickSupplierRequest
    {
        public string SupplierName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? VatNo { get; set; }
    }

    public class QuickSupplierUpdateRequest
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? VatNo { get; set; }
    }

    public class QuickDeleteRequest
    {
        public int Id { get; set; }
    }
}
