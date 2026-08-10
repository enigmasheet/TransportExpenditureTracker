using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportExpenditureTracker.Models;

public class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VehicleNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Type { get; set; }

    [MaxLength(100)]
    public string? Make { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    public int? Year { get; set; }

    [MaxLength(100)]
    public string? EngineNumber { get; set; }

    [MaxLength(100)]
    public string? ChassisNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CurrentMeterReading { get; set; }

    public int? AssignedDriverId { get; set; }

    [ForeignKey(nameof(AssignedDriverId))]
    public Driver? AssignedDriver { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}