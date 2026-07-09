using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SupplierName { get; set; } = null!;

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? VatNo { get; set; }
}
