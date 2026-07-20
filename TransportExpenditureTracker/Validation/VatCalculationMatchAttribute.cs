using System.ComponentModel.DataAnnotations;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class VatCalculationMatchAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not List<ExpenseDetailViewModel> details || details.Count == 0)
            return ValidationResult.Success;

        var errors = new List<string>();
        var index = 0;
        foreach (var d in details)
        {
            var rowErrors = new List<string>();

            if (d.Quantity > 0 && d.Rate > 0)
            {
                var computedTaxable = Math.Round(d.Quantity * d.Rate, 2);
                if (Math.Abs(computedTaxable - d.TaxableAmount) > 0.5m)
                {
                    rowErrors.Add($"Taxable ({d.TaxableAmount}) ≠ Qty×Rate ({computedTaxable})");
                }
            }

            if (d.TaxableAmount > 0 && d.VatAmount > 0)
            {
                var computedVat = Math.Round(d.TaxableAmount * AppConstants.VatRate, 2);
                if (Math.Abs(computedVat - d.VatAmount) > 1.0m)
                {
                    rowErrors.Add($"VAT ({d.VatAmount}) ≠ 13% of taxable ({computedVat})");
                }
            }

            if (d.TaxableAmount > 0 && d.VatAmount > 0 && d.TotalAmount > 0)
            {
                var computedTotal = Math.Round(d.TaxableAmount + d.VatAmount, 2);
                if (Math.Abs(computedTotal - d.TotalAmount) > 1.0m)
                {
                    rowErrors.Add($"Total ({d.TotalAmount}) ≠ Taxable+VAT ({computedTotal})");
                }
            }

            if (rowErrors.Count > 0)
            {
                errors.Add($"Row {index + 1}: {string.Join("; ", rowErrors)}");
            }

            index++;
        }

        return errors.Count > 0
            ? new ValidationResult(string.Join(" | ", errors))
            : ValidationResult.Success;
    }
}