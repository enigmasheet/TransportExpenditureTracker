namespace TransportExpenditureTracker.ViewModels;

public class ExpenseHeaderViewModel
{
    public int ExpenseId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string Miti { get; set; } = string.Empty;
    public DateTime? EnglishDate { get; set; }
    public string? FiscalYear { get; set; }
    public string? NepaliMonth { get; set; }
    public string? SupplierName { get; set; }
    public string? CategoryName { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Remarks { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
