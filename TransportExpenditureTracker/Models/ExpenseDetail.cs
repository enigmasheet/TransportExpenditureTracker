using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportExpenditureTracker.Models;

public class ExpenseDetail
{
    [Key]
    public int DetailId { get; set; }

    public int ExpenseId { get; set; }

    public int ItemId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxableAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VatAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public ExpenseHeader Expense { get; set; } = null!;

    public Item Item { get; set; } = null!;
}
