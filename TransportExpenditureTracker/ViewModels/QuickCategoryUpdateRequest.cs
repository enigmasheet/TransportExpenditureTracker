using System.Text.Json.Serialization;

namespace TransportExpenditureTracker.ViewModels;

public class QuickCategoryUpdateRequest
{
    [JsonRequired]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
