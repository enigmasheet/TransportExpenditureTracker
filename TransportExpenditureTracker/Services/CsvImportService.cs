using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class CsvImportService : ICsvImportService
{
    private readonly ApplicationDbContext _db;
    private readonly IExpenseService _expenseService;
    private readonly ICompanyService _companyService;

    public CsvImportService(ApplicationDbContext db, IExpenseService expenseService, ICompanyService companyService)
    {
        _db = db;
        _expenseService = expenseService;
        _companyService = companyService;
    }

    public async Task<CsvPreviewViewModel> PreviewAsync(IFormFile file)
    {
        var preview = new CsvPreviewViewModel();
        var vatRate = await _companyService.GetVatRateAsync();

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
                Quantity = GetDecimalValue(record, "Quantity") ?? 0,
                Rate = GetDecimalValue(record, "Rate") ?? 0,
                TaxableAmount = GetDecimalValue(record, "TaxableAmount") ?? GetDecimalValue(record, "Taxable Amount") ?? 0,
                VatAmount = GetDecimalValue(record, "VatAmount") ?? GetDecimalValue(record, "VAT Amount") ?? 0,
                TotalAmount = GetDecimalValue(record, "TotalAmount") ?? GetDecimalValue(record, "Total Amount") ?? 0
            };

            if (string.IsNullOrWhiteSpace(row.InvoiceNo))
                row.ValidationError = "Invoice number is required";
            else if (string.IsNullOrWhiteSpace(row.Miti))
                row.ValidationError = "Miti is required";
            else if (string.IsNullOrWhiteSpace(row.SupplierName))
                row.ValidationError = "Supplier name is required";
            else if (string.IsNullOrWhiteSpace(row.ItemName))
                row.ValidationError = "Item name is required";
            else if (NepaliDateHelper.ParseNepaliDate(row.Miti) is null)
                row.ValidationError = "Invalid Miti date format";
            else
            {
                var fiscalYearName = FiscalYearHelper.GetFiscalYearName(row.Miti);
                if (string.IsNullOrEmpty(fiscalYearName))
                {
                    row.ValidationError = "Could not determine fiscal year from Miti";
                }
                else
                {
                    var fiscalYear = await _db.FiscalYears.FirstOrDefaultAsync(f => f.Name == fiscalYearName);
                    if (fiscalYear is null)
                        row.ValidationError = $"Fiscal year '{fiscalYearName}' not found in system";
                    else
                    {
                        row.FiscalYearId = fiscalYear.Id;
                        row.FiscalYearName = fiscalYear.Name;

                        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == row.SupplierName);
                        if (supplier is not null)
                            row.IsDuplicate = await _expenseService.IsDuplicateInvoiceAsync(row.InvoiceNo, supplier.SupplierId, fiscalYear.Id);
                    }
                }

                if (row.Quantity <= 0)
                    row.ValidationError = "Quantity must be greater than 0";
                else if (row.Rate <= 0)
                    row.ValidationError = "Rate must be greater than 0";
                else if (row.TaxableAmount <= 0)
                    row.ValidationError = "Taxable amount must be greater than 0";

                if (string.IsNullOrEmpty(row.ValidationError))
                {
                    if (row.TaxableAmount > 0 && row.VatAmount > 0)
                    {
                        var expectedVat = Math.Round(row.TaxableAmount * vatRate, 2, MidpointRounding.AwayFromZero);
                        if (Math.Abs(row.VatAmount - expectedVat) > 1.0m)
                            row.IsVatMismatch = true;
                    }

                    if (row.TaxableAmount > 0 && row.VatAmount > 0 && row.TotalAmount > 0)
                    {
                        var expectedTotal = Math.Round(row.TaxableAmount + row.VatAmount, 2, MidpointRounding.AwayFromZero);
                        if (Math.Abs(row.TotalAmount - expectedTotal) > 1.0m)
                            row.IsTotalMismatch = true;
                    }
                }
            }

            rows.Add(row);
            rowIndex++;
        }

        var seenKeys = new HashSet<string>();
        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.ValidationError) && !row.IsDuplicate)
            {
                var key = $"{row.InvoiceNo}|{row.SupplierName}|{row.FiscalYearName}";
                if (!seenKeys.Add(key))
                {
                    row.IsCrossRowDuplicate = true;
                    row.ValidationError = "Duplicate invoice in same import file";
                }
            }
        }

        preview.Rows = rows;
        preview.TotalRows = rows.Count;
        preview.DuplicateCount = rows.Count(r => r.IsDuplicate || r.IsCrossRowDuplicate);
        preview.ErrorCount = rows.Count(r => !string.IsNullOrEmpty(r.ValidationError));
        preview.WarningCount = rows.Count(r => r.IsVatMismatch || r.IsTotalMismatch);

        return preview;
    }

    public async Task<ImportSummaryViewModel> ImportAsync(CsvPreviewViewModel preview, string userId, bool autoCreate)
    {
        var validRows = preview.Rows
            .Where(r => !r.IsDuplicate && !r.IsCrossRowDuplicate && string.IsNullOrEmpty(r.ValidationError))
            .ToList();

        return await _expenseService.ImportCsvAsync(validRows, userId, autoCreate);
    }

    private static string? GetStringValue(dynamic record, string key)
    {
        var dict = (IDictionary<string, object>)record;
        return dict.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static decimal? GetDecimalValue(dynamic record, string key)
    {
        var dict = (IDictionary<string, object>)record;
        if (dict.TryGetValue(key, out var value) && value is not null)
        {
            decimal result;
            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;
        }
        return null;
    }
}