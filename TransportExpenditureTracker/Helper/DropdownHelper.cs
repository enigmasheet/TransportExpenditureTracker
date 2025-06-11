using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportExpenditureTracker.Data;
using System.Linq;

namespace TransportExpenditureTracker.Helpers
{
    public static class DropdownHelper
    {
        public static void LoadFiscalYearAndMonths(ApplicationDbContext context, ViewDataDictionary viewData)
        {
            viewData["FiscalYears"] = new SelectList(
                context.FiscalYears.OrderByDescending(f => f.Name),
                "Name", "Name"
            );

            viewData["FiscalMonths"] = new SelectList(new[]
            {
                "Shrawan(4)", "Bhadra(5)", "Ashwin(6)", "Kartik(7)", "Mangsir(8)", "Poush(9)",
                "Magh(10)", "Falgun(11)", "Chaitra(12)", "Baisakh(1)", "Jestha(2)", "Ashadh(3)"
            });
        }

        public static void LoadDropdowns(ApplicationDbContext context, ViewDataDictionary viewData, int? selectedPartyId = null, int? selectedItemId = null)
        {
            var parties = context.Parties
                .Select(p => new
                {
                    p.PartyId,
                    DisplayText = $"{p.VatNo} - {p.PartyName}"
                })
                .ToList();

            viewData["PartyId"] = new SelectList(parties, "PartyId", "DisplayText", selectedPartyId);
            viewData["ItemId"] = new SelectList(context.Items, "ItemId", "ItemName", selectedItemId);
        }

    }

}
