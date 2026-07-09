using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Data.Seed;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services;
using TransportExpenditureTracker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Services
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IExportJobService, ExportJobService>();

// Converters
builder.Services.AddScoped<SupplierConverter>();
builder.Services.AddScoped<ExpenseConverter>();
builder.Services.AddScoped<ItemConverter>();
builder.Services.AddScoped<ExpenseCategoryConverter>();
builder.Services.AddScoped<ReportConverter>();

// Email + Background
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddHostedService<ExportBackgroundJob>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdminRole", policy => policy.RequireRole("SuperAdmin"));
});

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    await AppDbInitializer.SeedAsync(scope.ServiceProvider);
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

app.Run();
