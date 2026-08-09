using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models;

public class Company
{
    public const int CompanyId = 1;

    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(100)]
    public string? ShortName { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(50)]
    public string? PanNumber { get; set; }

    [MaxLength(50)]
    public string? VatNumber { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Province { get; set; }

    [MaxLength(50)]
    public string? District { get; set; }

    [MaxLength(50)]
    public string? Municipality { get; set; }

    [MaxLength(20)]
    public string? Ward { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? LogoPath { get; set; }

    [Range(0, 1, ErrorMessage = "VAT rate must be between 0 and 1")]
    public decimal DefaultVatRate { get; set; } = AppConstants.VatRate;

    [MaxLength(20)]
    public string FiscalYearStartMonth { get; set; } = "Shrawan";

    [MaxLength(10)]
    public string Currency { get; set; } = "NPR";

    [MaxLength(10)]
    public string CurrencySymbol { get; set; } = "Rs.";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}