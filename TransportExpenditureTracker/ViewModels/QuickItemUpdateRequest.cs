using System.Text.Json.Serialization;

namespace TransportExpenditureTracker.ViewModels;

public class QuickItemUpdateRequest
{
    [JsonRequired]
    public int ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string? Unit { get; set; }
}
