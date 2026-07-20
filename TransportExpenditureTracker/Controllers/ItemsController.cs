using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Helper.ControllerHelpers;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ItemsController(IItemDataManager itemDataManager, ItemConverter converter, IAuditService audit) : Controller
{
    private const string DeleteAction = "Delete";
    private static readonly string[] ItemNameRequired = ["Item name is required."];
    private static readonly string[] ItemNotFound = ["Item not found."];
    private static readonly string[] ItemReferenced = ["Cannot delete: item is referenced in existing expense records."];
    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Items", null));
        var items = await itemDataManager.GetAllAsync();
        var vms = items.Select(converter.ToViewModel).ToList();
        var countMap = await itemDataManager.GetDetailCountsAsync();
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
            await itemDataManager.AddAsync(item);
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var item = await itemDataManager.GetByIdAsync(id);
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
            var item = await itemDataManager.GetByIdAsync(id);
            if (item == null) return NotFound();
            item.ItemName = vm.ItemName;
            item.Unit = vm.Unit;
            await itemDataManager.UpdateAsync(item);
            TempData["Success"] = "Item updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var item = await itemDataManager.GetByIdAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        ViewData["DeleteConfirm"] = $"Are you sure you want to delete item '{item.ItemName}'?";
        return View(vm);
    }

    [HttpPost, ActionName(DeleteAction)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }
        var item = await itemDataManager.GetByIdAsync(id);
        if (item != null)
        {
            var inUse = await itemDataManager.IsReferencedAsync(id);
            if (inUse)
            {
                TempData["Error"] = $"Cannot delete '{item.ItemName}': it is referenced in existing expense records.";
                return RedirectToAction(nameof(Index));
            }
            await itemDataManager.DeleteAsync(id);
            await audit.LogAsync("Item", id.ToString(CultureInfo.InvariantCulture), DeleteAction, item.ItemName, null, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "");
            TempData["Success"] = "Item deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate([FromBody] QuickItemRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.ItemName))
            return Json(new { success = false, errors = new { itemName = ItemNameRequired } });

        var item = new Item { ItemName = request.ItemName, Unit = request.Unit };
        await itemDataManager.AddAsync(item);
        var displayText = item.ItemName + (string.IsNullOrEmpty(item.Unit) ? "" : $" ({item.Unit})");
        return Json(new { success = true, id = item.ItemId, text = displayText });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var item = await itemDataManager.GetByIdAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        return PartialView("_ItemEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickItemUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.ItemName))
            return Json(new { success = false, errors = new { itemName = ItemNameRequired } });

        var item = await itemDataManager.GetByIdAsync(request.ItemId);
        if (item == null)
            return Json(new { success = false, errors = new { general = ItemNotFound } });

        item.ItemName = request.ItemName;
        item.Unit = request.Unit;
        await itemDataManager.UpdateAsync(item);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var item = await itemDataManager.GetByIdAsync(id);
        if (item == null) return NotFound();
        var vm = new ItemViewModel { ItemId = item.ItemId, ItemName = item.ItemName, Unit = item.Unit ?? string.Empty };
        return PartialView("_ItemDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var item = await itemDataManager.GetByIdAsync(request.Id);
        if (item != null)
        {
            var inUse = await itemDataManager.IsReferencedAsync(request.Id);
            if (inUse)
                return Json(new { success = false, errors = new { general = ItemReferenced } });
            await itemDataManager.DeleteAsync(request.Id);
            await audit.LogAsync("Item", request.Id.ToString(CultureInfo.InvariantCulture), DeleteAction, item.ItemName, null, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "");
        }
        return Json(new { success = true });
    }
}
