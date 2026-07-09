using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class FiscalYear
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = null!;
}
