namespace TransportExpenditureTracker.Services.Interfaces;

public interface IFiscalCalendarService
{
    string? GetFiscalYearName(DateTime value);
    int GetQuarter(DateTime value);
    int GetNepaliMonthNumber(DateTime value);
    string? GetNepaliMonthName(DateTime value);
    string? GetNepaliMiti(DateTime value);
}