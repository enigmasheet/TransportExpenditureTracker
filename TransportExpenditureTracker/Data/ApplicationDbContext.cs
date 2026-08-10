using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ExpenseHeader> ExpenseHeaders => Set<ExpenseHeader>();
    public DbSet<ExpenseDetail> ExpenseDetails => Set<ExpenseDetail>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ExportQueue> ExportQueues => Set<ExportQueue>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Supplier>()
            .HasIndex(s => s.VatNo)
            .IsUnique()
            .HasFilter("[VatNo] IS NOT NULL");

        builder.Entity<ExpenseHeader>()
            .HasIndex(e => new { e.InvoiceNo, e.SupplierId, e.FiscalYearId })
            .IsUnique();

        builder.Entity<ExpenseHeader>()
            .HasOne(e => e.FiscalYearNav)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.Entity<ExpenseHeader>()
            .HasOne(e => e.Supplier)
            .WithMany()
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Entity<ExpenseHeader>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.Entity<ExpenseHeader>()
            .HasMany(e => e.Details)
            .WithOne(d => d.Expense)
            .HasForeignKey(d => d.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ExpenseDetail>()
            .HasOne(d => d.Item)
            .WithMany()
            .HasForeignKey(d => d.ItemId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.Entity<ExpenseDetail>()
            .HasOne(d => d.Expense)
            .WithMany(e => e.Details)
            .HasForeignKey(d => d.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Entity<ExpenseHeader>()
            .HasIndex(e => e.EnglishDate);

        builder.Entity<ExpenseHeader>()
            .HasIndex(e => e.CreatedAt);

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityName, a.EntityId });

        builder.Entity<ExportQueue>()
            .HasIndex(e => e.Status);

        builder.Entity<ExpenseDetail>(entity =>
        {
            entity.Property(d => d.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(d => d.Rate).HasColumnType("decimal(18,2)");
            entity.Property(d => d.TaxableAmount).HasColumnType("decimal(18,2)");
            entity.Property(d => d.VatAmount).HasColumnType("decimal(18,2)");
            entity.Property(d => d.TotalAmount).HasColumnType("decimal(18,2)");
        });

        builder.Entity<Company>(entity =>
        {
            entity.Property(c => c.DefaultVatRate).HasColumnType("decimal(18,4)");
        });

        builder.Entity<Vehicle>()
            .HasIndex(v => v.VehicleNumber)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Entity<Vehicle>()
            .HasOne(v => v.AssignedDriver)
            .WithMany()
            .HasForeignKey(v => v.AssignedDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Driver>()
            .HasIndex(d => d.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Entity<PaymentMethod>()
            .HasIndex(p => p.Name)
            .IsUnique();
    }
}
