using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces
{
    public interface IPdfGenerator
    {
        byte[] GenerateVatInvoiceReport(List<ReportRowViewModel> data,Company company);
    }
}
