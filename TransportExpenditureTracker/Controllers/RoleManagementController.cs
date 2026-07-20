using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Helper.ControllerHelpers;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Policy = "RequireSuperAdminRole")]
public class RoleManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext ctx, IAuditService audit) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    private static readonly string[] userNotFound = ["User not found."];

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("User Management", null));
        var users = await userManager.Users.ToListAsync();
        var roleAssignments = await ctx.UserRoles.ToListAsync();
        var roleNames = await ctx.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);
        var userRoles = users.Select(u => new UserRolesViewModel
        {
            UserId = u.Id,
            Email = u.Email ?? "",
            Roles = [.. roleAssignments
                .Where(ur => ur.UserId == u.Id)
                .Select(ur => roleNames.GetValueOrDefault(ur.RoleId, "")!)]
        }).ToList();
        return View(userRoles);
    }

    public async Task<IActionResult> ManageRoles(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        var model = (await roleManager.Roles.ToListAsync()).Select(async role => new ManageUserRolesViewModel
        {
            RoleName = role.Name ?? "",
            Selected = await userManager.IsInRoleAsync(user, role.Name ?? "")
        }).ToList();
        ViewBag.UserEmail = user.Email;
        ViewBag.UserId = userId;
        return PartialView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageRoles(List<ManageUserRolesViewModel> model, string userId)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Json(new { success = false, errors = new { UserId = userNotFound } });
        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        var selectedRoles = model.Where(r => r.Selected).Select(r => r.RoleName).ToList();
        if (selectedRoles.Count > 0)
        {
            await userManager.AddToRolesAsync(user, selectedRoles);
        }
        var removedRoles = currentRoles.Except(selectedRoles).ToList();
        var addedRoles = selectedRoles.Except(currentRoles).ToList();
        var message = $"Updated roles for {user.Email}: removed [{string.Join(", ", removedRoles)}], added [{string.Join(", ", addedRoles)}]";
        await audit.LogAsync("User", userId, "Update", null, null, CurrentUserId, message, "Information", null);
        return Json(new { success = true });
    }
}
