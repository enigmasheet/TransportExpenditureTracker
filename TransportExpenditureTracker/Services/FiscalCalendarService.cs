using NepDate;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Services;

public class FiscalCalendarService : IFiscalCalendarService
{
    public string? GetFiscalYearName(DateTime value)
    {
        var nepali = ToNepali(value);
        if (nepali is null) return null;

        return nepali.Value.Month >= 4
            ? $"{nepali.Value.Year}/{nepali.Value.Year % 100 + 1:D2}"
            : $"{nepali.Value.Year - 1}/{nepali.Value.Year % 100:D2}";
    }

    public int GetQuarter(DateTime value)
    {
        var month = GetNepaliMonthNumber(value);
        return month switch
        {
            >= 4 and <= 6 => 1,
            >= 7 and <= 9 => 2,
            >= 10 and <= 12 => 3,
            _ => 4
        };
    }

    public int GetNepaliMonthNumber(DateTime value)
    {
        return ToNepali(value)?.Month ?? 0;
    }

    public string? GetNepaliMonthName(DateTime value)
    {
        var month = GetNepaliMonthNumber(value);
        return month is >= 1 and <= 12 ? NepaliDateHelper.NepaliMonthNames[month - 1] : null;
    }

    public string? GetNepaliMiti(DateTime value)
    {
        var nepali = ToNepali(value);
        return nepali is null ? null : $"{nepali.Value.Year}/{nepali.Value.Month}/{nepali.Value.Day}";
    }

    private static NepaliDate? ToNepali(DateTime value)
    {
        try
        {
            return new NepaliDate(value);
        }
        catch (Exception)
        {
            return null;
        }
    }
}