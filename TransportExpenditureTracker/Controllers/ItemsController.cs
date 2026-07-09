using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
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
        var items = await _ctx.Items.OrderBy(i => i.ItemName).ToListAsync();
        var vms = items.Select(_converter.ToViewModel).ToList();
        return View(vms);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var item = new Item { ItemName = vm.ItemName, Unit = vm.Unit };
            _ctx.Items.Add(item);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Item created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
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
}