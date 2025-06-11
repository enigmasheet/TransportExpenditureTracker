using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentCompanyService _currentCompanyService;


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentCompanyService currentCompanyService)
            : base(options)
        {
            _currentCompanyService = currentCompanyService;
        }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<UserCompany> UserCompanies { get; set; }

       
        public int CurrentCompanyId { get; set; } = 0;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Company>()
                .HasIndex(c => c.VatNumber)
                .IsUnique();

            builder.Entity<UserCompany>()
                .HasIndex(uc => new { uc.UserId, uc.CompanyId })
                .IsUnique();

            builder.Entity<UserCompany>()
              .HasOne(uc => uc.User)
              .WithOne(u => u.UserCompany)
              .HasForeignKey<UserCompany>(uc => uc.UserId)
              .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<UserCompany>()
                .HasOne(uc => uc.Company)
                .WithMany(c => c.UserCompanies)
                .HasForeignKey(uc => uc.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Explicit FK config to fix multiple cascade paths error
            builder.Entity<Party>()
                .HasOne(p => p.Company)
                .WithMany(c => c.Parties)  // make sure Company class has Parties collection
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Party>()
                .HasIndex(p => new { p.CompanyId, p.VatNo })
                .IsUnique(); //Enforces uniqueness
            
            builder.Entity<Invoice>()
                .HasIndex(i => new { i.InvoiceNo, i.PartyId, i.CompanyId })
                .IsUnique();
            builder.Entity<Party>().HasQueryFilter(p => CurrentCompanyId == 0 || p.CompanyId == CurrentCompanyId);
            builder.Entity<Invoice>().HasQueryFilter(inv => CurrentCompanyId == 0 ||inv.CompanyId == CurrentCompanyId);

        }
    }  
}
