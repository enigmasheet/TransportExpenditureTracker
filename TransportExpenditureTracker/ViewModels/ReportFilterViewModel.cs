namespace TransportExpenditureTracker.ViewModels;

public class ReportFilterViewModel
{
    public string? FiscalYear { get; set; }
    public string? NepaliMonth { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? SupplierId { get; set; }
    public int? CategoryId { get; set; }
    public int? ItemId { get; set; }
    public string? InvoiceNo { get; set; }
    public string? Location { get; set; }
    public string? VatNo { get; set; }
    public string? PaymentMethod { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
