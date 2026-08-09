using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Validation;

namespace TransportExpenditureTracker.ViewModels;

public class ExpenseEntryViewModel : IValidatableObject
{
    public int ExpenseId { get; set; }

    [Required(ErrorMessage = "Invoice number is required")]
    [StringLength(50, ErrorMessage = "Invoice number cannot exceed 50 characters")]
    [RegularExpression(@"^[\w\-/]+$", ErrorMessage = "Invoice number can only contain letters, numbers, hyphens, and slashes")]
    public string InvoiceNo { get; set; } = string.Empty;

    [ValidMiti]
    public string Miti { get; set; } = string.Empty;

    [StringLength(20)]
    public string FiscalYear { get; set; } = string.Empty;

    [StringLength(20)]
    public string? NepaliMonth { get; set; }

    [Required(ErrorMessage = "Supplier is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid supplier")]
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }

    [Required(ErrorMessage = "Payment method is required")]
    [StringLength(30, ErrorMessage = "Payment method cannot exceed 30 characters")]
    public string PaymentMethod { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }

    [Required(ErrorMessage = "Fiscal year is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid fiscal year")]
    public int FiscalYearId { get; set; }

    [Required(ErrorMessage = "At least one detail row is required")]
    [MinLength(1, ErrorMessage = "At least one detail row is required")]
    [VatCalculationMatch]
    public List<ExpenseDetailViewModel> Details { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FiscalYearId > 0 && !string.IsNullOrWhiteSpace(Miti))
        {
            var db = validationContext.GetService(typeof(Data.ApplicationDbContext)) as Data.ApplicationDbContext;
            var fiscalYear = db?.FiscalYears.Find(FiscalYearId);
            if (fiscalYear is not null)
            {
                var fyFromMiti = Helper.FiscalYearHelper.GetFiscalYearName(Miti);
                if (!string.IsNullOrEmpty(fyFromMiti) && fyFromMiti != fiscalYear.Name)
                {
                    yield return new ValidationResult(
                        $"Fiscal year '{fiscalYear.Name}' does not match fiscal year '{fyFromMiti}' derived from Miti.",
                        [nameof(FiscalYearId), nameof(Miti)]);
                }
            }
        }

        if (Details is not null)
        {
            var itemIds = Details.Where(d => d.ItemId > 0).Select(d => d.ItemId).Distinct().ToList();
            Dictionary<int, string> itemNames = [];
            if (itemIds.Count > 0)
            {
                var db = validationContext.GetService(typeof(Data.ApplicationDbContext)) as Data.ApplicationDbContext;
                itemNames = db is null
                    ? []
                    : db.Items.AsNoTracking()
                        .Where(i => itemIds.Contains(i.ItemId))
                        .Select(i => new { i.ItemId, i.ItemName })
                        .ToList()
                        .ToDictionary(i => i.ItemId, i => i.ItemName);
            }

            for (int i = 0; i < Details.Count; i++)
            {
                var d = Details[i];

                if (d.ItemId <= 0)
                {
                    yield return new ValidationResult(
                        $"Row {i + 1}: Item is required.",
                        [nameof(Details)]);
                }

                if (d.Quantity <= 0)
                {
                    yield return new ValidationResult(
                        $"Row {i + 1}: Quantity must be greater than 0.",
                        [nameof(Details)]);
                }

                if (d.Rate <= 0)
                {
                    yield return new ValidationResult(
                        $"Row {i + 1}: Rate must be greater than 0.",
                        [nameof(Details)]);
                }

                if (d.TaxableAmount <= 0)
                {
                    yield return new ValidationResult(
                        $"Row {i + 1}: Taxable amount must be greater than 0.",
                        [nameof(Details)]);
                }

                if (string.IsNullOrWhiteSpace(d.ItemName) && itemNames.TryGetValue(d.ItemId, out var name))
                    d.ItemName = name;
            }
        }
    }
}