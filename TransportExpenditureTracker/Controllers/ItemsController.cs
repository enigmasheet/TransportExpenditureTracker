using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ItemsController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly ItemConverter _converter;

    public ItemsController(ApplicationDbContext ctx, ItemConverter converter)
    {
        _ctx = ctx;
        _converter = converter;
    }

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Items", null));
        var items = await _ctx.Items.OrderBy(i => i.ItemName).ToListAsync();
        var vms = items.Select(_converter.ToViewModel).ToList();
        var allDetails = await _ctx.ExpenseDetails
            .Select(d => new { d.ItemId, d.ExpenseId })
            .ToListAsync();
        var detailCounts = allDetails
            .GroupBy(d => d.ItemId)
            .Select(g => new { Id = g.Key, Count = g.Select(d => d.ExpenseId).Distinct().Count() })
            .ToList();
        var countMap = detailCounts.ToDictionary(c => c.Id, c => c.Count);
        foreach (var vm in vms)
            vm.ExpenseCount = countMap.GetValueOrDefault(vm.ItemId);
        return View(vms);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] ItemViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var item = new Item { ItemName = vm.ItemName, Unit = vm.Unit };
            _ctx.Items.Add(item);
            await _ctx.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _ctx.Items.FindAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemViewModel vm)
    {
        if (id != vm.ItemId) return NotFound();
        if (ModelState.IsValid)
        {
            var item = await _ctx.Items.FindAsync(id);
            if (item == null) return NotFound();
            item.ItemName = vm.ItemName;
            item.Unit = vm.Unit;
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Item updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _ctx.Items.FindAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        ViewData["DeleteConfirm"] = $"Are you sure you want to delete item '{item.ItemName}'?";
        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _ctx.Items.FindAsync(id);
        if (item != null)
        {
            _ctx.Items.Remove(item);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Item deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
            return Json(new { success = false, errors = new { itemName = new[] { "Item name is required." } } });

        var item = new Item { ItemName = request.ItemName, Unit = request.Unit };
        _ctx.Items.Add(item);
        await _ctx.SaveChangesAsync();
        var displayText = item.ItemName + (string.IsNullOrEmpty(item.Unit) ? "" : $" ({item.Unit})");
        return Json(new { success = true, id = item.ItemId, text = displayText });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var item = await _ctx.Items.FindAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        return PartialView("_ItemEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickItemUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
            return Json(new { success = false, errors = new { itemName = new[] { "Item name is required." } } });

        var item = await _ctx.Items.FindAsync(request.ItemId);
        if (item == null)
            return Json(new { success = false, errors = new { general = new[] { "Item not found." } } });

        item.ItemName = request.ItemName;
        item.Unit = request.Unit;
        await _ctx.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        var item = await _ctx.Items.FindAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        return PartialView("_ItemDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        var item = await _ctx.Items.FindAsync(request.Id);
        if (item != null)
        {
            _ctx.Items.Remove(item);
            await _ctx.SaveChangesAsync();
        }
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

    public class QuickItemRequest
    {
        public string ItemName { get; set; } = string.Empty;
        public string? Unit { get; set; }
    }

    public class QuickItemUpdateRequest
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Unit { get; set; }
    }

    public class QuickDeleteRequest
    {
        public int Id { get; set; }
    }
}
