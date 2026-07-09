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
            FiscalYear = h.FiscalYear,
            NepaliMonth = h.NepaliMonth,
            SupplierName = h.Supplier?.SupplierName ?? string.Empty,
            CategoryName = h.Category?.CategoryName ?? string.Empty,
            PaymentMethod = h.PaymentMethod,
            Remarks = h.Remarks,
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
            FiscalYear = h.FiscalYear,
            NepaliMonth = h.NepaliMonth,
            SupplierId = h.SupplierId,
            CategoryId = h.CategoryId,
            PaymentMethod = h.PaymentMethod,
            Remarks = h.Remarks
        };

        if (h.Details != null)
        {
            vm.Details = h.Details.Select(ToDetailViewModel).ToList();
        }

        return vm;
    }

    public ExpenseHeader ToHeaderModel(ExpenseEntryViewModel vm)
    {
        return new ExpenseHeader
        {
            InvoiceNo = vm.InvoiceNo,
            Miti = vm.Miti,
            FiscalYear = vm.FiscalYear,
            NepaliMonth = vm.NepaliMonth,
            SupplierId = vm.SupplierId,
            CategoryId = vm.CategoryId,
            Remarks = vm.Remarks,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public ExpenseDetail ToDetailModel(ExpenseEntryViewModel vm, int headerId)
    {
        var d = vm.Details.FirstOrDefault();

        return new ExpenseDetail
        {
            ExpenseId = headerId,
            ItemId = d?.ItemId ?? 0,
            Quantity = d?.Quantity ?? 0,
            Rate = d?.Rate ?? 0,
            TaxableAmount = d?.TaxableAmount ?? 0,
            VatAmount = d?.VatAmount ?? 0,
            TotalAmount = d?.TotalAmount ?? 0
        };
    }

    public void UpdateHeaderModel(ExpenseEntryViewModel vm, ExpenseHeader existing)
    {
        existing.InvoiceNo = vm.InvoiceNo;
        existing.Miti = vm.Miti;
        existing.FiscalYear = vm.FiscalYear;
        existing.NepaliMonth = vm.NepaliMonth;
        existing.SupplierId = vm.SupplierId;
        existing.CategoryId = vm.CategoryId;
        existing.PaymentMethod = vm.PaymentMethod;
        existing.Remarks = vm.Remarks;
        existing.UpdatedAt = DateTime.UtcNow;
    }
}
