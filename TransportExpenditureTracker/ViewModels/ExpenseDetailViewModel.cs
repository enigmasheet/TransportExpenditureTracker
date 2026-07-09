using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class ExpenseDetailViewModel
{
    public int DetailId { get; set; }

    [Required(ErrorMessage = "Item is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid item")]
    public int ItemId { get; set; }

    public string? ItemName { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Quantity cannot be negative")]
    public decimal Quantity { get; set; }

    [Range(0, 999999999.999, ErrorMessage = "Rate cannot be negative")]
    public decimal Rate { get; set; }

    [Range(0.01, 999999999.99, ErrorMessage = "Taxable amount must be between 0.01 and 999,999,999.99")]
    public decimal TaxableAmount { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "VAT amount cannot be negative")]
    public decimal VatAmount { get; set; }

    [Range(0.01, 999999999.99, ErrorMessage = "Total amount must be between 0.01 and 999,999,999.99")]
    public decimal TotalAmount { get; set; }
}