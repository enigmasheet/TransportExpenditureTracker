using System.Globalization;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Helper;

public static class FiscalYearHelper
{
    public static FiscalYear? GetFiscalYear(string miti, List<FiscalYear> fiscalYears)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return null;

        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]), CultureInfo.InvariantCulture);
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]), CultureInfo.InvariantCulture);

        string fyName = nepaliMonth >= 4
            ? $"{nepaliYear}/{GetEndingYearSuffix(nepaliYear)}"
            : $"{nepaliYear - 1}/{GetEndingYearSuffix(nepaliYear - 1)}";

        return fiscalYears.FirstOrDefault(f => f.Name == fyName);
    }

    public static string GetFiscalYearName(string miti)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return string.Empty;

        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]), CultureInfo.InvariantCulture);
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]), CultureInfo.InvariantCulture);

        return nepaliMonth >= 4
            ? $"{nepaliYear}/{GetEndingYearSuffix(nepaliYear)}"
            : $"{nepaliYear - 1}/{GetEndingYearSuffix(nepaliYear - 1)}";
    }

    private static string GetEndingYearSuffix(int startingYear)
    {
        var lastTwoDigits = startingYear % 100 + 1;
        return lastTwoDigits >= 100 ? "00" : lastTwoDigits.ToString("D2", CultureInfo.InvariantCulture);
    }
}