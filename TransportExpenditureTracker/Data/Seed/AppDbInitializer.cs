﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TransportExpenditureTracker.Data.Seed
{
    public static class AppDbInitializer
    {
        public static async Task SeedRolesAdminFiscalYearsItemsAndCompaniesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Roles
            string[] roles = { "SuperAdmin", "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Super Admin User
            var superAdminEmail = "superadmin@gmail.com";
            var superAdminPassword = "Super@123";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");

                    // Optional: Give SuperAdmin all roles
                    foreach (var role in roles)
                    {
                        if (!await userManager.IsInRoleAsync(superAdminUser, role))
                        {
                            await userManager.AddToRoleAsync(superAdminUser, role);
                        }
                    }
                }
                else
                {
                    throw new Exception("Failed to create Super Admin user: " + string.Join(", ", result.Errors));
                }
            }

            // 3. Seed Admin User
            var adminEmail = "abhaymandal321@gmail.com";
            var adminPassword = "Admin@123";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors));
                }
            }

            // 4. Seed Normal User
            var normalUserEmail = "user@gmail.com";
            var normalUserPassword = "User@123";

            var normalUser = await userManager.FindByEmailAsync(normalUserEmail);
            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = normalUserEmail,
                    Email = normalUserEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(normalUser, normalUserPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
                else
                {
                    throw new Exception("Failed to create normal user: " + string.Join(", ", result.Errors));
                }
            }

            // 5. Seed Fiscal Years
            if (!await context.FiscalYears.AnyAsync())
            {
                context.FiscalYears.AddRange(
                    new FiscalYear { Name = "2081/82" }
                );
                await context.SaveChangesAsync();
            }

            // 6. Seed Items
            if (!await context.Items.AnyAsync())
            {
                var items = new List<Item>
                {
                    new Item { ItemName = "Diesel" },
                    new Item { ItemName = "Petrol" },
                    new Item { ItemName = "Stationary" },
                    new Item { ItemName = "Parts" },
                    new Item { ItemName = "Mobile" },
                    new Item { ItemName = "Internet" },
                    new Item { ItemName = "Tyres" },
                    new Item { ItemName = "Servicing" },
                    new Item { ItemName = "Battery" },
                    new Item { ItemName = "Engine Oil" },
                    new Item { ItemName = "Insurance" },
                    new Item { ItemName = "Toll Charges" },
                    new Item { ItemName = "Maintenance" },
                    new Item { ItemName = "Cleaning" },
                    new Item { ItemName = "Parking" }
                };

                context.Items.AddRange(items);
                await context.SaveChangesAsync();
            }

            // 7. Seed Companies (Important for VAT-based registration)
            if (!await context.Companies.AnyAsync())
            {
                var companies = new List<Company>
                {
                    new Company { Name = "Sunya Technologies Pvt Ltd", VatNumber = "VAT123456",Location="Lalitpur",ContactNumber="9806886202" },
                    new Company { Name = "Transport Expenditure Ltd", VatNumber = "VAT654321",Location="Kathmandu",ContactNumber="9800000000" }
                };

                context.Companies.AddRange(companies);
                await context.SaveChangesAsync();
            }
        }
    }
}
