using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;
namespace TransportExpenditureTracker.Controllers
{
    [Authorize(Policy = "RequireSuperAdminRole")]
    public class RoleManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleManagementController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
     

        // List all users and their roles
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var companyIds = await _context.UserCompanies
                    .Where(uc => uc.UserId == user.Id)
                    .Select(uc => uc.CompanyId)
                    .ToListAsync();

                var companyNames = await _context.Companies
                    .Where(c => companyIds.Contains(c.Id))
                    .Select(c => c.Name)
                    .ToListAsync();
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = new List<string>(await _userManager.GetRolesAsync(user)),
                    CompanyNames = companyNames

                };
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }

        // GET: Manage roles for a specific user
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            var model = new List<ManageUserRolesViewModel>();

            foreach (var role in allRoles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleName = role.Name,
                    Selected = await _userManager.IsInRoleAsync(user, role.Name)
                };
                model.Add(userRolesViewModel);
            }

            ViewBag.userId = userId;
            ViewBag.userEmail = user.Email;
            return View(model);
        }


        // POST: Update roles for a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ManageUserCompanies(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var allCompanies = await _context.Companies.AsNoTracking().ToListAsync();
            var userCompanyIds = await _context.UserCompanies
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CompanyId)
                .ToListAsync();

            var viewModel = new ManageUserCompaniesPageViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Companies = allCompanies.Select(c => new ManageUserCompaniesViewModel
                {
                    CompanyId = c.Id,
                    CompanyName = c.Name,
                    Selected = userCompanyIds.Contains(c.Id)
                }).ToList()
            };

            return PartialView("_ManageUserCompaniesModal", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserCompanies(ManageUserCompaniesPageViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var existing = _context.UserCompanies.Where(uc => uc.UserId == model.UserId);
            _context.UserCompanies.RemoveRange(existing);
            await _context.SaveChangesAsync();

            var selectedCompanyIds = model.Companies
                .Where(c => c.Selected)
                .Select(c => c.CompanyId)
                .ToList();

            foreach (var companyId in selectedCompanyIds)
            {
                _context.UserCompanies.Add(new UserCompany
                {
                    UserId = model.UserId,
                    CompanyId = companyId
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "User companies updated successfully.";

            return RedirectToAction(nameof(Index));
        }



    }

}
