namespace TransportExpenditureTracker.ViewModels;

public class ReportRowViewModel
{
    public int Sno { get; set; }
    public string Miti { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? VatNo { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime EnglishDate { get; set; }
    public string? FiscalYear { get; set; }
    public string? NepaliMonth { get; set; }
    public string? PaymentMethod { get; set; }
    public int SupplierId { get; set; }
    public int CategoryId { get; set; }
    public int ItemId { get; set; }
}
