using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportExpenditureTracker.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNo { get; set; } = null!;

        [MaxLength(50)]
        [Display(Name = "Nepali Miti")]
        public string NepaliMiti { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "English Date")]
        public DateTime Miti { get; set; }

        [ForeignKey(nameof(Party))]
        [Display(Name = "Party")]
        public int PartyId { get; set; }

        [ForeignKey(nameof(Item))]
        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be positive")]
        [Display(Name = "Rate per Unit")]
        public decimal? Rate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "Taxable Amount")]
        public decimal TaxableAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "VAT Amount")]
        public decimal VatAmount { get; set; }

        [NotMapped]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount => TaxableAmount + VatAmount;

        [Display(Name = "Fiscal Year")]
        public string? FiscalYear { get; set; }

        [Display(Name = "Fiscal Month")]
        public string? FiscalMonth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "Total Invoice Amount")]
        public decimal TotalInvoiceAmount { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [ValidateNever]
        public Party Party { get; set; } = null!;

        [ValidateNever]
        public Item Item { get; set; } = null!;
    }
}
