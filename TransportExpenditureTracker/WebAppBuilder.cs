using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Data.Seed;
using TransportExpenditureTracker.DataManagers;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker;

public static class WebAppBuilder
{
    public static WebApplication Build(string[] args, string? connectionString = null)
    {
        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TransportExpenditureTracker", "logs");
        Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.WithProperty("Application", "TransportExpenditureTracker")
            .WriteTo.File(
                Path.Combine(logFolder, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            connectionString ??= builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IExpenseService, ExpenseService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IReportExportService, ReportExportService>();
            builder.Services.AddScoped<ICsvImportService, CsvImportService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IExportJobService, ExportJobService>();

            builder.Services.AddScoped<IItemDataManager, ItemDataManager>();
            builder.Services.AddScoped<IExpenseCategoryDataManager, ExpenseCategoryDataManager>();
            builder.Services.AddScoped<ISupplierDataManager, SupplierDataManager>();
            builder.Services.AddScoped<IExpenseDataManager, ExpenseDataManager>();
            builder.Services.AddScoped<IDashboardDataManager, DashboardDataManager>();
            builder.Services.AddScoped<IReportDataManager, ReportDataManager>();

            builder.Services.AddScoped<SupplierConverter>();
            builder.Services.AddScoped<ExpenseConverter>();
            builder.Services.AddScoped<ItemConverter>();
            builder.Services.AddScoped<ExpenseCategoryConverter>();

            builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
            builder.Services.AddTransient<EmailSender>();
            builder.Services.AddHostedService<ExportBackgroundJob>();

            builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
            builder.Services.AddControllersWithViews().AddApplicationPart(typeof(WebAppBuilder).Assembly);
            builder.Services.AddRazorPages();

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("RequireSuperAdminRole", policy => policy.RequireRole("SuperAdmin"));

            var app = builder.Build();
            app.UseSerilogRequestLogging();

            using (var scope = app.Services.CreateScope())
            {
                AppDbInitializer.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            }

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
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();

            app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}").WithStaticAssets();
            app.MapRazorPages().WithStaticAssets();

            Log.Information("Application started successfully");
            return app;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            throw new InvalidOperationException("Application failed to start", ex);
        }
    }
}