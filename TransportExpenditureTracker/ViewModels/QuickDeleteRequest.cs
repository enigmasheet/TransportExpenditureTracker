using System.Text.Json.Serialization;

namespace TransportExpenditureTracker.ViewModels;

public class QuickDeleteRequest
{
    [JsonRequired]
    public int Id { get; set; }
}
