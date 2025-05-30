using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportExpenditureTracker.Models
{
    public class Party
    {
        public Party()
        {
            Invoices = new HashSet<Invoice>();
        }

        [Key]
        public int PartyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartyName { get; set; } = null!;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? VatNo { get; set; }

        // Navigation property: One party can have many invoices
        public ICollection<Invoice> Invoices { get; set; }
    }

    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNo { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Miti { get; set; }

        [ForeignKey(nameof(Party))]
        public int PartyId { get; set; }
       
        [ValidateNever] 
        public Party Party { get; set; } = null!;

        // InvoiceItem properties merged here:
        [Required]
        [MaxLength(200)]
        public string Item { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be positive")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal TaxableAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal VatAmount { get; set; }

        [NotMapped]
        public decimal TotalAmount => TaxableAmount + VatAmount;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal TotalInvoiceAmount { get; set; }  // optional, you can store or compute this

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

}
