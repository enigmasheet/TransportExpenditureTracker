using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Converters;

public class ReportConverter
{
    public ReportRowViewModel ToReportRow(ExpenseDetail d, ExpenseHeader h)
    {
        return new ReportRowViewModel
        {
            Sno = 0,
            Miti = h.Miti,
            InvoiceNo = h.InvoiceNo,
            SupplierName = h.Supplier?.SupplierName ?? string.Empty,
            Location = h.Supplier?.Location,
            VatNo = h.Supplier?.VatNo,
            ItemName = d.Item?.ItemName ?? string.Empty,
            CategoryName = h.Category?.CategoryName ?? string.Empty,
            Quantity = d.Quantity,
            Unit = d.Item?.Unit ?? string.Empty,
            Rate = d.Rate,
            TaxableAmount = d.TaxableAmount,
            VatAmount = d.VatAmount,
            TotalAmount = d.TotalAmount
        };
    }
}
