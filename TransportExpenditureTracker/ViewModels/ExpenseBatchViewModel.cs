using System.ComponentModel.DataAnnotations;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.Validation;

namespace TransportExpenditureTracker.ViewModels;

public class ExpenseBatchViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Fiscal year is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a fiscal year")]
    public int FiscalYearId { get; set; }

    [Required(ErrorMessage = "Month is required")]
    public string NepaliMonth { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "At least one expense row is required")]
    public List<ExpenseBatchRowViewModel> Rows { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var db = validationContext.GetService(typeof(Data.ApplicationDbContext)) as Data.ApplicationDbContext;
        var expenseService = validationContext.GetService(typeof(IExpenseService)) as IExpenseService;

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

            var mitiParts = row.Miti.Split('/');
            var mitiMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(mitiParts[1]));
            var monthIndex = Array.IndexOf(NepaliDateHelper.NepaliMonthNames, NepaliMonth) + 1;
            if (mitiMonth != monthIndex)
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Miti month ({mitiMonth}) does not match selected month ({NepaliMonth}).", [rowPrefix]);
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

            if (db is not null && row.SupplierId > 0 && expenseService is not null && !string.IsNullOrWhiteSpace(row.InvoiceNo))
            {
                var isDup = expenseService.IsDuplicateInvoiceAsync(row.InvoiceNo, row.SupplierId, FiscalYearId).Result;
                if (isDup)
                {
                    yield return new ValidationResult($"Row {row.RowIndex}: Invoice '{row.InvoiceNo}' already exists for this supplier in this fiscal year.", [rowPrefix]);
                }
            }

            var key = $"{row.InvoiceNo}|{row.SupplierId}";
            if (!string.IsNullOrWhiteSpace(row.InvoiceNo) && row.SupplierId > 0 && !seenKeys.Add(key))
            {
                yield return new ValidationResult($"Row {row.RowIndex}: Duplicate invoice '{row.InvoiceNo}' for same supplier within this batch.", [rowPrefix]);
            }
        }
    }
}