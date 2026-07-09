namespace TransportExpenditureTracker.ViewModels;

public class ExpenseDetailViewModel
{
    public int DetailId { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
