using System.ComponentModel.DataAnnotations;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.ViewModels;

public class ExpenseBatchViewModel : IValidatableObject
{
    [MinLength(1, ErrorMessage = "At least one expense row is required")]
    public List<ExpenseBatchRowViewModel> Rows { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var seenKeys = new HashSet<string>();

        for (int i = 0; i < Rows.Count; i++)
        {
            var row = Rows[i];
            row.RowIndex = i + 1;
            var rowPrefix = $"Rows[{i}]";

            if (string.IsNullOrWhiteSpace(row.Miti))
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Miti is required.", [rowPrefix]);
                continue;
            }

            if (NepaliDateHelper.ParseNepaliDate(row.Miti) is null)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Invalid Miti format (use YYYY/MM/DD).", [rowPrefix]);
                continue;
            }

            if (row.SupplierId <= 0)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Supplier is required.", [rowPrefix]);
            }

            if (row.ItemId <= 0)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Item is required.", [rowPrefix]);
            }

            if (row.Quantity <= 0)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Quantity must be greater than 0.", [rowPrefix]);
            }

            if (row.Rate <= 0)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Rate must be greater than 0.", [rowPrefix]);
            }

            var taxable = Math.Round(row.Quantity * row.Rate, 2);
            if (Math.Abs(taxable - row.TaxableAmount) > 0.5m && row.TaxableAmount > 0)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Taxable amount ({row.TaxableAmount}) does not match Qty×Rate ({taxable}).", [rowPrefix]);
            }

            var key = $"{row.InvoiceNo}|{row.SupplierId}";
            if (!string.IsNullOrWhiteSpace(row.InvoiceNo) && row.SupplierId > 0 && !seenKeys.Add(key))
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Duplicate invoice '{row.InvoiceNo}' for same supplier within this batch.", [rowPrefix]);
            }
        }
    }
}