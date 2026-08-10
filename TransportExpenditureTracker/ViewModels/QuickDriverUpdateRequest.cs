using System.Text.Json.Serialization;

namespace TransportExpenditureTracker.ViewModels;

public class QuickDriverUpdateRequest
{
    [JsonRequired]
    public int DriverId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; } = true;
}