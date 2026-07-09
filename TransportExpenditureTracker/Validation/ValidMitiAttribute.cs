using System.ComponentModel.DataAnnotations;
using TransportExpenditureTracker.Helper;

namespace TransportExpenditureTracker.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class ValidMitiAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string miti || string.IsNullOrWhiteSpace(miti))
        {
            return new ValidationResult("Miti (Nepali date) is required.");
        }

        if (NepaliDateHelper.ParseNepaliDate(miti) is null)
        {
            return new ValidationResult("Miti must be a valid Nepali date in YYYY/MM/DD format (e.g., 2082/04/01).");
        }

        return ValidationResult.Success;
    }
}