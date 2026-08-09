using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Services;

public class CompanyService(ApplicationDbContext db) : ICompanyService
{
    public async Task<Company?> GetAsync()
    {
        return await db.Companies.AsNoTracking().OrderBy(c => c.Id).FirstOrDefaultAsync();
    }

    public async Task<bool> IsSetupCompleteAsync()
    {
        return await db.Companies.AnyAsync();
    }

    public async Task<Company> SaveAsync(Company company)
    {
        var existing = await db.Companies.OrderBy(c => c.Id).FirstOrDefaultAsync();

        if (existing is null)
        {
            company.Id = Company.CompanyId;
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = null;
            db.Companies.Add(company);
        }
        else
        {
            existing.Name = company.Name;
            existing.ShortName = company.ShortName;
            existing.RegistrationNumber = company.RegistrationNumber;
            existing.PanNumber = company.PanNumber;
            existing.VatNumber = company.VatNumber;
            existing.Address = company.Address;
            existing.Province = company.Province;
            existing.District = company.District;
            existing.Municipality = company.Municipality;
            existing.Ward = company.Ward;
            existing.Phone = company.Phone;
            existing.Email = company.Email;
            existing.Website = company.Website;
            existing.LogoPath = company.LogoPath;
            existing.DefaultVatRate = company.DefaultVatRate;
            existing.FiscalYearStartMonth = company.FiscalYearStartMonth;
            existing.Currency = company.Currency;
            existing.CurrencySymbol = company.CurrencySymbol;
            existing.UpdatedAt = DateTime.UtcNow;
            company = existing;
        }

        await db.SaveChangesAsync();
        return company;
    }

    public async Task<decimal> GetVatRateAsync()
    {
        var company = await db.Companies.AsNoTracking().OrderBy(c => c.Id).FirstOrDefaultAsync();
        return company?.DefaultVatRate > 0 ? company.DefaultVatRate : AppConstants.VatRate;
    }
}