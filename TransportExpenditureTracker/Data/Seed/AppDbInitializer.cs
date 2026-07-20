using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Data.Seed;

public static class AppDbInitializer
{
    private static readonly Action<ILogger, string, Exception?> LogSuperAdminCreated =
        LoggerMessage.Define<string>(LogLevel.Information, EventIds.SuperAdminCreated, "SuperAdmin user '{Email}' created.");

    private static readonly Action<ILogger, string, string, Exception?> LogSuperAdminFailed =
        LoggerMessage.Define<string, string>(LogLevel.Warning, EventIds.SuperAdminFailed, "Failed to create SuperAdmin user '{Email}': {Errors}");

    private static readonly Action<ILogger, Exception?> LogSuperAdminNotConfigured =
        LoggerMessage.Define(LogLevel.Warning, EventIds.SuperAdminNotConfigured, "SuperAdmin credentials not configured. Set SuperAdmin:Email and SuperAdmin:Password in configuration (appsettings, user secrets, or env vars).");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var context = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();

        await context.Database.MigrateAsync();

        string[] roles = ["SuperAdmin", "Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminSection = config.GetSection("SuperAdmin");
        var adminEmail = adminSection["Email"];
        var adminPassword = adminSection["Password"];

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var appUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminSection["FullName"] ?? "Super Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(appUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(appUser, "SuperAdmin");
                    await userManager.AddToRoleAsync(appUser, "Admin");
                    LogSuperAdminCreated(logger, adminEmail, null);
                }
                else
                {
                    LogSuperAdminFailed(logger, adminEmail, string.Join(", ", result.Errors.Select(e => e.Description)), null);
                }
            }
        }
        else
        {
            LogSuperAdminNotConfigured(logger, null);
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
                new Item { ItemName = "Engine Oil", Unit = "" },
                new Item { ItemName = "Tyres", Unit = "Piece" },
                new Item { ItemName = "Battery", Unit = "Piece" },
                new Item { ItemName = "Parts", Unit = "Piece" },
                new Item { ItemName = "Mobile", Unit = "Piece" },
                new Item { ItemName = "Stationary", Unit = "Piece" },
                new Item { ItemName = "Internet", Unit = "Months" },
                new Item { ItemName = "Insurance", Unit = "Year" },
                new Item { ItemName = "Toll Charges", Unit = "" },
                new Item { ItemName = "Parking", Unit = "" },
                new Item { ItemName = "Cleaning", Unit = "" },
                new Item { ItemName = "Servicing", Unit = "" },
                new Item { ItemName = "Maintenance", Unit = "" }
            );
            await context.SaveChangesAsync();
        }
    }

    private static partial class EventIds
    {
        public static readonly EventId SuperAdminCreated = new(1, nameof(SuperAdminCreated));
        public static readonly EventId SuperAdminFailed = new(2, nameof(SuperAdminFailed));
        public static readonly EventId SuperAdminNotConfigured = new(3, nameof(SuperAdminNotConfigured));
    }
}
