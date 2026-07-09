using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class ExpenseBatchRowViewModel
{
    public int RowIndex { get; set; }

    [Required(ErrorMessage = "Miti is required")]
    [StringLength(20)]
    public string Miti { get; set; } = string.Empty;

    [Required(ErrorMessage = "Invoice number is required")]
    [StringLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supplier is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a supplier")]
    public int SupplierId { get; set; }

    public string? SupplierName { get; set; }

    [Required(ErrorMessage = "Item is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select an item")]
    public int ItemId { get; set; }

    public string? ItemName { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Quantity cannot be negative")]
    public decimal Quantity { get; set; }

    [Range(0, 999999999.999, ErrorMessage = "Rate cannot be negative")]
    public decimal Rate { get; set; }

    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public int? CategoryId { get; set; }
    public string? PaymentMethod { get; set; }
}