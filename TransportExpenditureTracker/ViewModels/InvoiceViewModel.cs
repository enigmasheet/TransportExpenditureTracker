using System;
using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels
{
    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNo { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Miti { get; set; }

        [Required]
        [Display(Name = "Party")]
        public int PartyId { get; set; }

        [Required]
        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be positive")]
        public decimal Rate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TaxableAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VatAmount { get; set; }

        public decimal TotalAmount => TaxableAmount + VatAmount;

        [Display(Name = "Fiscal Year")]
        public string? FiscalYear { get; set; }

        [Display(Name = "Fiscal Month")]
        public string? FiscalMonth { get; set; }

        public decimal TotalInvoiceAmount { get; set; }

        // Optional: For dropdowns (e.g., SelectList in View)
        public string? PartyName { get; set; }
        public string? ItemName { get; set; }
    }
}
