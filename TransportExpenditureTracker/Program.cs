using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Data.Seed;
using TransportExpenditureTracker.Mappings;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker
{
    public class Program
    {
        public static async Task Main(string[] args) 
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure())
            );

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<IdentityRole>() // Add Roles
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
            builder.Services.AddScoped<IPartyService, PartyService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentCompanyService, CurrentCompanyService>();
            builder.Services.AddControllersWithViews();
           
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireSuperAdminRole", policy => policy.RequireRole("SuperAdmin"));
            });

            var app = builder.Build();

            // Seed roles and admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await AppDbInitializer.SeedRolesAdminFiscalYearsItemsAndCompaniesAsync(services);
            }

            // Middleware configuration
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseMiddleware<TransportExpenditureTracker.Helper.CurrentCompanyMiddleware>();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Invoices}/{action=CreateMultiple}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
