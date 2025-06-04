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
        public string InvoiceNo { get; set; } = null!;
        
        
        [MaxLength(50)]
        public string NepaliMiti { get; set; } = null!; // Nepali date in string format


        [Required]
        [DataType(DataType.Date)]
        public DateTime Miti { get; set; }

        [ForeignKey(nameof(Party))]
        public int PartyId { get; set; }
       
        [ForeignKey(nameof(Item))]
        public int ItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be positive")]
        public decimal? Rate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal TaxableAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal VatAmount { get; set; }

        [NotMapped]
        public decimal TotalAmount => TaxableAmount + VatAmount;

        public string? FiscalYear { get; set; } 

        public string? FiscalMonth { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal TotalInvoiceAmount { get; set; }  // optional, you can store or compute this

        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }

        [ValidateNever]
        public Party Party { get; set; } = null!;

        [ValidateNever]
        public Item Item { get; set; } = null!;
    }

}
