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

        public int PartyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Item { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be positive")]
        public decimal Rate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TaxableAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VatAmount { get; set; }

        // You may or may not want this on the view model,
        // since it is computed or stored separately.
        public decimal TotalInvoiceAmount { get; set; }
    }
}
