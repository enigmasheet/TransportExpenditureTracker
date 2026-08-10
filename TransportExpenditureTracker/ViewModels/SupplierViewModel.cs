using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class SupplierViewModel
{
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Supplier name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Supplier name must be between 2 and 100 characters")]
    public string SupplierName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
    public string? Location { get; set; }

    [StringLength(50, ErrorMessage = "VAT number cannot exceed 50 characters")]
    public string? VatNo { get; set; }

    public bool IsFuelSupplier { get; set; }

    public bool IsActive { get; set; } = true;

    public int ExpenseCount { get; set; }
}