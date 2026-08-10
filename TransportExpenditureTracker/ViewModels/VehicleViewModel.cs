using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class VehicleViewModel
{
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Vehicle number is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Vehicle number cannot exceed 50 characters")]
    public string VehicleNumber { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
    public string? Type { get; set; }

    [StringLength(100, ErrorMessage = "Make cannot exceed 100 characters")]
    public string? Make { get; set; }

    [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    public string? Model { get; set; }

    [Range(1950, 2100, ErrorMessage = "Enter a valid year")]
    public int? Year { get; set; }

    [StringLength(100, ErrorMessage = "Engine number cannot exceed 100 characters")]
    public string? EngineNumber { get; set; }

    [StringLength(100, ErrorMessage = "Chassis number cannot exceed 100 characters")]
    public string? ChassisNumber { get; set; }

    [Range(0, 9_999_999, ErrorMessage = "Enter a valid meter reading")]
    public decimal? CurrentMeterReading { get; set; }

    public int? AssignedDriverId { get; set; }

    public string? DriverName { get; set; }

    public bool IsActive { get; set; } = true;

    public int ExpenseCount { get; set; }
}