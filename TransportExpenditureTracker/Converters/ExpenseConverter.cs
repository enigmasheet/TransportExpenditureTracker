using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class ExpenseConverter
{
    public ExpenseHeaderViewModel ToHeaderViewModel(ExpenseHeader h)
    {
        return new ExpenseHeaderViewModel
        {
            ExpenseId = h.ExpenseId,
            InvoiceNo = h.InvoiceNo,
            Miti = h.Miti,
            EnglishDate = h.EnglishDate,
            FiscalYear = h.FiscalYearNav?.Name ?? string.Empty,
            NepaliMonth = h.NepaliMonth,
            SupplierName = h.Supplier?.SupplierName ?? string.Empty,
            CategoryName = h.Category?.CategoryName ?? string.Empty,
            PaymentMethod = h.PaymentMethod,
            Remarks = h.Remarks,
            TotalAmount = h.Details?.Sum(d => d.TotalAmount) ?? 0,
            CreatedAt = h.CreatedAt,
            UpdatedAt = h.UpdatedAt
        };
    }

    public ExpenseDetailViewModel ToDetailViewModel(ExpenseDetail d)
    {
        return new ExpenseDetailViewModel
        {
            DetailId = d.DetailId,
            ItemId = d.ItemId,
            ItemName = d.Item?.ItemName ?? string.Empty,
            Quantity = d.Quantity,
            Rate = d.Rate,
            TaxableAmount = d.TaxableAmount,
            VatAmount = d.VatAmount,
            TotalAmount = d.TotalAmount
        };
    }

    public ExpenseEntryViewModel ToEntryViewModel(ExpenseHeader h)
    {
        var vm = new ExpenseEntryViewModel
        {
            ExpenseId = h.ExpenseId,
            InvoiceNo = h.InvoiceNo,
            Miti = h.Miti,
            FiscalYear = h.FiscalYearNav?.Name ?? string.Empty,
            FiscalYearId = h.FiscalYearId,
            NepaliMonth = h.NepaliMonth,
            SupplierId = h.SupplierId,
            SupplierName = h.Supplier?.SupplierName,
            CategoryId = h.CategoryId,
            CategoryName = h.Category?.CategoryName,
            PaymentMethod = h.PaymentMethod,
            Remarks = h.Remarks
        };

        if (h.Details != null)
        {
            vm.Details = h.Details.Select(ToDetailViewModel).ToList();
        }

        return vm;
    }

    public void UpdateHeaderModel(ExpenseEntryViewModel vm, ExpenseHeader existing)
    {
        existing.InvoiceNo = vm.InvoiceNo;
        existing.Miti = vm.Miti;
        existing.FiscalYearId = vm.FiscalYearId;
        existing.NepaliMonth = vm.NepaliMonth;
        existing.SupplierId = vm.SupplierId;
        existing.CategoryId = vm.CategoryId;
        existing.PaymentMethod = vm.PaymentMethod;
        existing.Remarks = vm.Remarks;
        existing.UpdatedAt = DateTime.UtcNow;
    }
}
