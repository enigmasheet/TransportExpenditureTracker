using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.ViewModels
{
    public class InvoiceMonthly
    {
        public string? FiscalYear { get; set; }
        public string? FiscalMonth { get; set; }
        public List<Invoice>? Invoices { get; set; }
    }

}
