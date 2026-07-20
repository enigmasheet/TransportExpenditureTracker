using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels;

public class ExpenseCategoryViewModel
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string CategoryName { get; set; } = string.Empty;

    public int ExpenseCount { get; set; }
}