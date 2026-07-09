using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class ItemViewModel
{
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Item name must be between 2 and 100 characters")]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
    public string Unit { get; set; } = string.Empty;
}