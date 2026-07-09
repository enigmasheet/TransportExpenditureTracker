using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Data.Seed;

public static class AppDbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        string[] roles = ["SuperAdmin", "Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var users = new (string Email, string Password, string Role, string FullName)[]
        {
            ("superadmin@ems.com", "Super@123", "SuperAdmin", "Super Admin"),
            ("admin@ems.com", "Admin@123", "Admin", "Admin User"),
            ("user@ems.com", "User@123", "User", "Normal User"),
        };

        foreach (var (email, password, role, fullName) in users)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var appUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(appUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(appUser, role);
                }
            }
        }

        if (!await context.FiscalYears.AnyAsync())
        {
            context.FiscalYears.AddRange(
                new FiscalYear { Name = "2081/82" },
                new FiscalYear { Name = "2082/83" },
                new FiscalYear { Name = "2083/84" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.ExpenseCategories.AnyAsync())
        {
            context.ExpenseCategories.AddRange(
                new ExpenseCategory { CategoryName = "Fuel" },
                new ExpenseCategory { CategoryName = "Office Expense" },
                new ExpenseCategory { CategoryName = "Maintenance" },
                new ExpenseCategory { CategoryName = "Electricity" },
                new ExpenseCategory { CategoryName = "Internet" },
                new ExpenseCategory { CategoryName = "Miscellaneous" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Items.AnyAsync())
        {
            context.Items.AddRange(
                new Item { ItemName = "Diesel", Unit = "Liters" },
                new Item { ItemName = "Petrol", Unit = "Liters" },
                new Item { ItemName = "Engine Oil", Unit = "Liters" },
                new Item { ItemName = "Tyres", Unit = "Piece" },
                new Item { ItemName = "Battery", Unit = "Piece" },
                new Item { ItemName = "Parts", Unit = "Piece" },
                new Item { ItemName = "Mobile", Unit = "Piece" },
                new Item { ItemName = "Stationary", Unit = "Piece" },
                new Item { ItemName = "Internet", Unit = "Months" },
                new Item { ItemName = "Insurance", Unit = "Year" },
                new Item { ItemName = "Toll Charges", Unit = "Each" },
                new Item { ItemName = "Parking", Unit = "Each" },
                new Item { ItemName = "Cleaning", Unit = "Each" },
                new Item { ItemName = "Servicing", Unit = "Hours" },
                new Item { ItemName = "Maintenance", Unit = "Hours" }
            );
            await context.SaveChangesAsync();
        }
    }
}
