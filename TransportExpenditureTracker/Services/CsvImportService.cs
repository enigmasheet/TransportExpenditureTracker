using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class CsvImportService : ICsvImportService
{
    private readonly ApplicationDbContext _db;
    private readonly IExpenseService _expenseService;

    public CsvImportService(ApplicationDbContext db, IExpenseService expenseService)
    {
        _db = db;
        _expenseService = expenseService;
    }

    public async Task<CsvPreviewViewModel> PreviewAsync(IFormFile file)
    {
        var preview = new CsvPreviewViewModel();

        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        });

        var records = csv.GetRecords<dynamic>().ToList();
        var rows = new List<CsvRowViewModel>();
        int rowIndex = 1;

        foreach (var record in records)
        {
            var row = new CsvRowViewModel
            {
                RowIndex = rowIndex,
                Miti = GetStringValue(record, "Miti"),
                InvoiceNo = GetStringValue(record, "InvoiceNo") ?? GetStringValue(record, "Invoice No"),
                SupplierName = GetStringValue(record, "SupplierName") ?? GetStringValue(record, "Supplier Name"),
                Location = GetStringValue(record, "Location"),
                VatNo = GetStringValue(record, "VatNo") ?? GetStringValue(record, "VAT No"),
                ItemName = GetStringValue(record, "ItemName") ?? GetStringValue(record, "Item Name"),
                Quantity = GetDecimalValue(record, "Quantity"),
                Rate = GetDecimalValue(record, "Rate"),
                TaxableAmount = GetDecimalValue(record, "TaxableAmount") ?? GetDecimalValue(record, "Taxable Amount"),
                VatAmount = GetDecimalValue(record, "VatAmount") ?? GetDecimalValue(record, "VAT Amount"),
                TotalAmount = GetDecimalValue(record, "TotalAmount") ?? GetDecimalValue(record, "Total Amount")
            };

            if (string.IsNullOrWhiteSpace(row.InvoiceNo))
                row.ValidationError = "Invoice number is required";
            else if (string.IsNullOrWhiteSpace(row.Miti))
                row.ValidationError = "Miti is required";
            else if (string.IsNullOrWhiteSpace(row.SupplierName))
                row.ValidationError = "Supplier name is required";
            else if (string.IsNullOrWhiteSpace(row.ItemName))
                row.ValidationError = "Item name is required";
            else
            {
                var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == row.SupplierName);
                if (supplier is null)
                {
                    var duplicateCheck = await _expenseService.IsDuplicateInvoiceAsync(row.InvoiceNo, 0);
                }
                else
                {
                    row.IsDuplicate = await _expenseService.IsDuplicateInvoiceAsync(row.InvoiceNo, supplier.SupplierId);
                }
            }

            rows.Add(row);
            rowIndex++;
        }

        preview.Rows = rows;
        preview.TotalRows = rows.Count;
        preview.DuplicateCount = rows.Count(r => r.IsDuplicate);
        preview.ErrorCount = rows.Count(r => !string.IsNullOrEmpty(r.ValidationError));

        return preview;
    }

    public async Task<ImportSummaryViewModel> ImportAsync(CsvPreviewViewModel preview, string userId, bool autoCreate)
    {
        var validRows = preview.Rows
            .Where(r => !r.IsDuplicate && string.IsNullOrEmpty(r.ValidationError))
            .ToList();

        return await _expenseService.ImportCsvAsync(validRows, userId, autoCreate);
    }

    private static string? GetStringValue(dynamic record, string key)
    {
        var dict = (IDictionary<string, object>)record;
        return dict.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static decimal GetDecimalValue(dynamic record, string key)
    {
        var dict = (IDictionary<string, object>)record;
        if (dict.TryGetValue(key, out var value) && value is not null)
        {
            decimal result;
            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;
        }
        return 0;
    }
}
