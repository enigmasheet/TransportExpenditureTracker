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
            ? $"{nepaliYear}/{nepaliYear % 100 + 1:D2}"
            : $"{nepaliYear - 1}/{nepaliYear % 100:D2}";

        return fiscalYears.FirstOrDefault(f => f.Name == fyName);
    }

    public static string GetFiscalYearName(string miti)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return string.Empty;

        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]), CultureInfo.InvariantCulture);
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]), CultureInfo.InvariantCulture);

        return nepaliMonth >= 4
            ? $"{nepaliYear}/{nepaliYear % 100 + 1:D2}"
            : $"{nepaliYear - 1}/{nepaliYear % 100:D2}";
    }
}