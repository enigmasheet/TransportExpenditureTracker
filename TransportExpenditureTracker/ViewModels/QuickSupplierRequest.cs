namespace TransportExpenditureTracker.ViewModels;

public class QuickSupplierRequest
{
    public string SupplierName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? VatNo { get; set; }
}
