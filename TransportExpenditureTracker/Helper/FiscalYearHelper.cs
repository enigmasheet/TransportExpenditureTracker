using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Helper;

public static class FiscalYearHelper
{
    public static FiscalYear? GetFiscalYear(string miti, List<FiscalYear> fiscalYears)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return null;

        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]));
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]));

        string fyName = nepaliMonth >= 4
            ? $"{nepaliYear}/{nepaliYear + 1}"
            : $"{nepaliYear - 1}/{nepaliYear}";

        return fiscalYears.FirstOrDefault(f => f.Name == fyName);
    }

    public static string GetFiscalYearName(string miti)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return string.Empty;

        var nepaliYear = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[0]));
        var nepaliMonth = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]));

        return nepaliMonth >= 4
            ? $"{nepaliYear}/{nepaliYear + 1}"
            : $"{nepaliYear - 1}/{nepaliYear}";
    }
}