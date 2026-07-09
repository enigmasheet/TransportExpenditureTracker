namespace TransportExpenditureTracker.ViewModels;

public class CsvRowViewModel
{
    public int RowIndex { get; set; }
    public string Miti { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? VatNo { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsDuplicate { get; set; }
    public string? ValidationError { get; set; }
}
