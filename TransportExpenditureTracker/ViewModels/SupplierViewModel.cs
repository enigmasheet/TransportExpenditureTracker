namespace TransportExpenditureTracker.ViewModels;

public class SupplierViewModel
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? VatNo { get; set; }
}
