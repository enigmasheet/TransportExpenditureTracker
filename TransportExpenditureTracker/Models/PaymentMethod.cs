using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class PaymentMethod
{
    [Key]
    public int PaymentMethodId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}