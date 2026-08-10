using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Options;

namespace TransportExpenditureTracker.Data.Seed;

public static class AppDbInitializer
{
    private static readonly Action<ILogger, string, Exception?> LogSuperAdminCreated =
        LoggerMessage.Define<string>(LogLevel.Information, EventIds.SuperAdminCreated, "SuperAdmin user '{Email}' created.");

    private static readonly Action<ILogger, string, string, Exception?> LogSuperAdminFailed =
        LoggerMessage.Define<string, string>(LogLevel.Warning, EventIds.SuperAdminFailed, "Failed to create SuperAdmin user '{Email}': {Errors}");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var context = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var options = sp.GetRequiredService<IOptions<SuperAdminOptions>>().Value;
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

        if (await userManager.FindByEmailAsync(options.Email) == null)
        {
            var appUser = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                FullName = options.FullName,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(appUser, options.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "SuperAdmin");
                await userManager.AddToRoleAsync(appUser, "Admin");
                LogSuperAdminCreated(logger, options.Email, null);
            }
            else
            {
                LogSuperAdminFailed(logger, options.Email, string.Join(", ", result.Errors.Select(e => e.Description)), null);
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
                new ExpenseCategory { CategoryName = "Vehicle Repair" },
                new ExpenseCategory { CategoryName = "Maintenance" },
                new ExpenseCategory { CategoryName = "Spare Parts" },
                new ExpenseCategory { CategoryName = "Tyres" },
                new ExpenseCategory { CategoryName = "Insurance" },
                new ExpenseCategory { CategoryName = "Road/Toll" },
                new ExpenseCategory { CategoryName = "Driver Salary" },
                new ExpenseCategory { CategoryName = "Salary" },
                new ExpenseCategory { CategoryName = "Rent" },
                new ExpenseCategory { CategoryName = "Electricity" },
                new ExpenseCategory { CategoryName = "Office" },
                new ExpenseCategory { CategoryName = "Internet" },
                new ExpenseCategory { CategoryName = "Bank Charges" },
                new ExpenseCategory { CategoryName = "Tax" },
                new ExpenseCategory { CategoryName = "Miscellaneous" },
                new ExpenseCategory { CategoryName = "Other" }
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

        if (!await context.PaymentMethods.AnyAsync())
        {
            context.PaymentMethods.AddRange(
                new PaymentMethod { Name = "Cash", SortOrder = 1 },
                new PaymentMethod { Name = "Bank", SortOrder = 2 },
                new PaymentMethod { Name = "Cheque", SortOrder = 3 },
                new PaymentMethod { Name = "eSewa", SortOrder = 4 },
                new PaymentMethod { Name = "Khalti", SortOrder = 5 },
                new PaymentMethod { Name = "Mobile Banking", SortOrder = 6 },
                new PaymentMethod { Name = "Other", SortOrder = 7 }
            );
            await context.SaveChangesAsync();
        }
    }

    private static partial class EventIds
    {
        public static readonly EventId SuperAdminCreated = new(1, nameof(SuperAdminCreated));
        public static readonly EventId SuperAdminFailed = new(2, nameof(SuperAdminFailed));
    }
}
