using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            // Use a stub to avoid service injection issues
            var stubCompanyService = new DesignTimeCompanyService();
            return new ApplicationDbContext(optionsBuilder.Options, stubCompanyService);
        }
    }

    // Dummy company service for migrations
    public class DesignTimeCompanyService : ICurrentCompanyService
    {
        public int CompanyId => 0; // Use a default ID that exists in your seed data
    }
}
