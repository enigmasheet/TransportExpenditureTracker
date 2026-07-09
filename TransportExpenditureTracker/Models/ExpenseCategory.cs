using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class ExpenseCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = null!;
}
