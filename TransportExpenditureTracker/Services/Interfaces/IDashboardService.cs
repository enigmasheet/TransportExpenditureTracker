using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
