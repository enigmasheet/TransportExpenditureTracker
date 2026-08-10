using System.Text.Json.Serialization;

namespace TransportExpenditureTracker.ViewModels;

public class QuickVehicleUpdateRequest
{
    [JsonRequired]
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public decimal? CurrentMeterReading { get; set; }
    public int? AssignedDriverId { get; set; }
    public bool IsActive { get; set; } = true;
}