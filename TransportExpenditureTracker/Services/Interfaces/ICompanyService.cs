using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface ICompanyService
{
    Task<Company?> GetAsync();
    Task<bool> IsSetupCompleteAsync();
    Task<Company> SaveAsync(Company company);
    Task<decimal> GetVatRateAsync();
}