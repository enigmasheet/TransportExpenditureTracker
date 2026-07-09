namespace TransportExpenditureTracker.ViewModels;

public class ExpenseEntryViewModel
{
    public int ExpenseId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string Miti { get; set; } = string.Empty;
    public string? FiscalYear { get; set; }
    public string? NepaliMonth { get; set; }
    public int SupplierId { get; set; }
    public int CategoryId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Remarks { get; set; }
    public List<ExpenseDetailViewModel> Details { get; set; } = [];
}
