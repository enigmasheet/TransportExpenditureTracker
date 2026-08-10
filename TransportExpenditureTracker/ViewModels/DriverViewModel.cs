using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class DriverViewModel
{
    public int DriverId { get; set; }

    [Required(ErrorMessage = "Driver name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Driver name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "License number cannot exceed 50 characters")]
    public string? LicenseNumber { get; set; }

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    public DateTime? HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int VehicleCount { get; set; }
}