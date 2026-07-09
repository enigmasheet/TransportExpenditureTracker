using NepDate;
using System.Text;

namespace TransportExpenditureTracker.Helper;

public static class NepaliDateHelper
{
    public static string ConvertToEnglishDigits(string nepaliNumber)
    {
        if (string.IsNullOrEmpty(nepaliNumber)) return nepaliNumber;

        var nepaliDigits = new Dictionary<char, char>
        {
            {'\u0966', '0'}, {'\u0967', '1'}, {'\u0968', '2'}, {'\u0969', '3'}, {'\u096A', '4'},
            {'\u096B', '5'}, {'\u096C', '6'}, {'\u096D', '7'}, {'\u096E', '8'}, {'\u096F', '9'}
        };

        var result = new StringBuilder();

        foreach (var c in nepaliNumber)
        {
            result.Append(nepaliDigits.TryGetValue(c, out var englishDigit) ? englishDigit : c);
        }

        return result.ToString();
    }

    public static DateTime? ParseNepaliDate(string nepaliMiti)
    {
        try
        {
            var parts = nepaliMiti.Split('/');
            if (parts.Length != 3) return null;

            var year = int.Parse(ConvertToEnglishDigits(parts[0]));
            var month = int.Parse(ConvertToEnglishDigits(parts[1]));
            var day = int.Parse(ConvertToEnglishDigits(parts[2]));

            var nepaliDate = new NepaliDate(year, month, day);
            return nepaliDate.EnglishDate;
        }
        catch
        {
            return null;
        }
    }
}
