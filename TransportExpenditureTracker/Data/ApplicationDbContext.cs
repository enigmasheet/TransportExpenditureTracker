using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

    }
}
