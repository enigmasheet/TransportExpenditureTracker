using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class ExpenseHeader
{
    [Key]
    public int ExpenseId { get; set; }

    [Required]
    [MaxLength(50)]
    public string InvoiceNo { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Miti { get; set; } = null!;

    public DateTime EnglishDate { get; set; }

    [MaxLength(20)]
    public string? NepaliMonth { get; set; }

    public int SupplierId { get; set; }

    public int CategoryId { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PaymentMethod { get; set; } = null!;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int FiscalYearId { get; set; }

    public FiscalYear FiscalYearNav { get; set; } = null!;

    public Supplier Supplier { get; set; } = null!;

    public ExpenseCategory Category { get; set; } = null!;

    public ICollection<ExpenseDetail> Details { get; set; } = new List<ExpenseDetail>();
}
