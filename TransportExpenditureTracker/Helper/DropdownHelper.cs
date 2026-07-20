using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Helper;

public static class DropdownHelper
{
    public static void LoadFiscalYears(ApplicationDbContext ctx, ViewDataDictionary vd, int? selected = null)
    {
        var years = ctx.FiscalYears.OrderByDescending(f => f.Id).ToList();
        vd["FiscalYears"] = new SelectList(years, "Id", "Name", selected);
    }

    public static void LoadFiscalYears(List<FiscalYear> fiscalYears, ViewDataDictionary vd, int? selected = null)
    {
        vd["FiscalYears"] = new SelectList(fiscalYears, "Id", "Name", selected);
    }

    public static void LoadNepaliMonths(ViewDataDictionary vd)
    {
        vd["NepaliMonths"] = new SelectList(NepaliDateHelper.NepaliMonthNames);
    }

    public static void LoadPaymentMethods(ViewDataDictionary vd)
    {
        var methods = new List<string> { "Cash", "Bank", "Cheque", "eSewa", "Khalti" };
        vd["PaymentMethods"] = new SelectList(methods);
    }

    public static void LoadSuppliers(ApplicationDbContext ctx, ViewDataDictionary vd, int? selected = null)
    {
        var suppliers = ctx.Suppliers.OrderBy(s => s.SupplierName).ToList();
        vd["Suppliers"] = new SelectList(suppliers, "SupplierId", "SupplierName", selected);
    }

    public static void LoadSuppliers(List<Supplier> suppliers, ViewDataDictionary vd, int? selected = null)
    {
        vd["Suppliers"] = new SelectList(suppliers, "SupplierId", "SupplierName", selected);
    }

    public static void LoadCategories(ApplicationDbContext ctx, ViewDataDictionary vd, int? selected = null)
    {
        var categories = ctx.ExpenseCategories.OrderBy(c => c.CategoryName).ToList();
        vd["Categories"] = new SelectList(categories, "CategoryId", "CategoryName", selected);
    }

    public static void LoadCategories(List<ExpenseCategory> categories, ViewDataDictionary vd, int? selected = null)
    {
        vd["Categories"] = new SelectList(categories, "CategoryId", "CategoryName", selected);
    }

    public static void LoadItems(ApplicationDbContext ctx, ViewDataDictionary vd, int? selected = null)
    {
        var items = ctx.Items.OrderBy(i => i.ItemName).ToList();
        vd["Items"] = new SelectList(items, "ItemId", "ItemName", selected);
    }

    public static void LoadItems(List<Item> items, ViewDataDictionary vd, int? selected = null)
    {
        vd["Items"] = new SelectList(items, "ItemId", "ItemName", selected);
    }
}
