namespace TransportExpenditureTracker.ViewModels;

public class DashboardViewModel
{
    public decimal TotalExpenditure { get; set; }
    public decimal TotalVatPaid { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public int TotalInvoices { get; set; }
    public decimal ThisMonthExpenses { get; set; }
    public decimal ThisFiscalYearExpenses { get; set; }
    public string? TopSupplier { get; set; }
    public List<string> FiscalYears { get; set; } = new();
    public string? SelectedFiscalYear { get; set; }
}
