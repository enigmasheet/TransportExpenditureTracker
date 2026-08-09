using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class ExpenseService(ApplicationDbContext db, ExpenseConverter converter, IAuditService audit) : IExpenseService
{
    public async Task<List<ExpenseHeaderViewModel>> GetAllAsync()
    {
        var headers = await db.ExpenseHeaders
            .AsNoTracking()
            .Include(h => h.Supplier)
            .Include(h => h.Category)
            .Include(h => h.FiscalYearNav)
            .Include(h => h.Details)
            .ThenInclude(d => d.Item)
            .OrderByDescending(h => h.EnglishDate)
            .ToListAsync();

        return [.. headers.Select(converter.ToHeaderViewModel)];
    }

    public async Task<ExpenseEntryViewModel?> GetByIdAsync(int id)
    {
        var header = await db.ExpenseHeaders
            .AsNoTracking()
            .Include(h => h.Supplier)
            .Include(h => h.Category)
            .Include(h => h.FiscalYearNav)
            .Include(h => h.Details)
            .ThenInclude(d => d.Item)
            .FirstOrDefaultAsync(h => h.ExpenseId == id);

        return header is null ? null : converter.ToEntryViewModel(header);
    }

    public async Task<bool> UpdateAsync(ExpenseEntryViewModel vm, string userId)
    {
        var existing = await db.ExpenseHeaders
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.ExpenseId == vm.ExpenseId);

        if (existing is null) return false;

        var oldValues = JsonSerializer.Serialize(converter.ToEntryViewModel(existing));

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            existing.InvoiceNo = vm.InvoiceNo;
            existing.Miti = vm.Miti;
            existing.EnglishDate = NepaliDateHelper.ParseNepaliDate(vm.Miti) ?? existing.EnglishDate;
            existing.NepaliMonth = vm.NepaliMonth;
            existing.SupplierId = vm.SupplierId;
            existing.CategoryId = vm.CategoryId;
            existing.FiscalYearId = vm.FiscalYearId;
            existing.PaymentMethod = vm.PaymentMethod ?? string.Empty;
            existing.Remarks = vm.Remarks;
            existing.UpdatedAt = DateTime.UtcNow;

            db.ExpenseDetails.RemoveRange(existing.Details);
            await db.SaveChangesAsync();

            if (vm.Details is not null)
            {
                foreach (var d in vm.Details)
                {
                    var detail = CreateDetail(existing.ExpenseId, d);
                    db.ExpenseDetails.Add(detail);
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            var newValues = JsonSerializer.Serialize(vm);
            await audit.LogAsync("ExpenseHeader", existing.ExpenseId.ToString(CultureInfo.InvariantCulture), "Update", oldValues, newValues, userId);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var existing = await db.ExpenseHeaders
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.ExpenseId == id);

        if (existing is null) return false;

        var oldValues = JsonSerializer.Serialize(converter.ToEntryViewModel(existing));

        db.ExpenseHeaders.Remove(existing);
        await db.SaveChangesAsync();

        await audit.LogAsync("ExpenseHeader", id.ToString(CultureInfo.InvariantCulture), "Delete", oldValues, null, userId);
        return true;
    }

    public async Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId, int fiscalYearId, int? excludeId = null)
    {
        var query = db.ExpenseHeaders
            .Where(h => h.InvoiceNo == invoiceNo && h.SupplierId == supplierId && h.FiscalYearId == fiscalYearId);
        if (excludeId.HasValue)
            query = query.Where(h => h.ExpenseId != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<ImportSummaryViewModel> ImportCsvAsync(List<CsvRowViewModel> rows, string userId, bool autoCreate)
    {
        var summary = new ImportSummaryViewModel();

        foreach (var row in rows)
        {
            try
            {
                if (SkipDuplicateOrInvalid(row, summary))
                    continue;

                var supplier = await ResolveSupplierAsync(row, autoCreate, summary);
                if (supplier is null) continue;

                var item = await ResolveItemAsync(row, autoCreate, summary);
                if (item is null) continue;

                var fiscalYear = await ResolveFiscalYearAsync(row, summary);
                if (fiscalYear is null) continue;

                var englishDate = NepaliDateHelper.ParseNepaliDate(row.Miti) ?? DateTime.UtcNow;
                var defaultCategoryId = await GetDefaultCategoryIdAsync();

                var header = new ExpenseHeader
                {
                    InvoiceNo = row.InvoiceNo,
                    Miti = row.Miti,
                    EnglishDate = englishDate,
                    NepaliMonth = GetSafeNepaliMonth(row.Miti),
                    FiscalYearId = fiscalYear.Id,
                    SupplierId = supplier.SupplierId,
                    CategoryId = defaultCategoryId,
                    PaymentMethod = "Cash",
                    CreatedAt = DateTime.UtcNow
                };
                db.ExpenseHeaders.Add(header);
                await db.SaveChangesAsync();

                var detail = CreateDetailFromRow(header.ExpenseId, item.ItemId, row);
                db.ExpenseDetails.Add(detail);
                await db.SaveChangesAsync();

                AddWarnings(row, summary);

                summary.Inserted++;
            }
            catch (Exception ex)
            {
                summary.Errors++;
                summary.ErrorMessages.Add($"Row {row.RowIndex}: {ex.Message}");
            }
        }

        await audit.LogAsync("CsvImport", "batch", "Create", null, null, userId, $"CSV import completed: {summary.Inserted} inserted, {summary.Skipped} skipped, {summary.Errors} errors", summary.Errors > 0 ? "Error" : "Information", null);
        return summary;
    }

    public async Task<ImportSummaryViewModel> BatchCreateAsync(ExpenseBatchViewModel vm, string userId)
    {
        using var transaction = await db.Database.BeginTransactionAsync();
        var summary = new ImportSummaryViewModel();
        var inserted = 0;

        var allFiscalYears = await db.FiscalYears.ToListAsync();

        try
        {
            foreach (var row in vm.Rows)
            {
                try
                {
                    if (row.SupplierId <= 0 || row.ItemId <= 0 || string.IsNullOrWhiteSpace(row.Miti))
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Missing required fields");
                        continue;
                    }

                    var fiscalYear = FiscalYearHelper.GetFiscalYear(row.Miti, allFiscalYears);
                    if (fiscalYear is null)
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Could not determine fiscal year from Miti");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(row.InvoiceNo) &&
                        await IsDuplicateInvoiceAsync(row.InvoiceNo, row.SupplierId, fiscalYear.Id))
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Invoice '{row.InvoiceNo}' already exists for this supplier and fiscal year");
                        continue;
                    }

                    var englishDate = NepaliDateHelper.ParseNepaliDate(row.Miti) ?? DateTime.UtcNow;

                    var supplier = await db.Suppliers.FindAsync(row.SupplierId);
                    if (supplier is null)
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Supplier not found");
                        continue;
                    }

                    var item = await db.Items.FindAsync(row.ItemId);
                    if (item is null)
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Item not found");
                        continue;
                    }

                    var taxableAmount = RoundMoney(row.Quantity * row.Rate);
                    var vatAmount = RoundMoney(taxableAmount * AppConstants.VatRate);
                    var totalAmount = RoundMoney(taxableAmount + vatAmount);

                    var header = new ExpenseHeader
                    {
                        InvoiceNo = row.InvoiceNo,
                        Miti = row.Miti,
                        EnglishDate = englishDate,
                        FiscalYearId = fiscalYear.Id,
                        NepaliMonth = GetSafeNepaliMonth(row.Miti),
                        SupplierId = row.SupplierId,
                        CategoryId = row.CategoryId ?? await GetDefaultCategoryIdAsync(),
                        PaymentMethod = row.PaymentMethod ?? "Cash",
                        CreatedAt = DateTime.UtcNow
                    };
                    db.ExpenseHeaders.Add(header);
                    await db.SaveChangesAsync();

                    var detail = new ExpenseDetail
                    {
                        ExpenseId = header.ExpenseId,
                        ItemId = row.ItemId,
                        Quantity = row.Quantity,
                        Rate = row.Rate,
                        TaxableAmount = taxableAmount,
                        VatAmount = vatAmount,
                        TotalAmount = totalAmount
                    };
                    db.ExpenseDetails.Add(detail);
                    await db.SaveChangesAsync();

                    inserted++;
                }
                catch (Exception ex)
                {
                    summary.Errors++;
                    summary.ErrorMessages.Add($"Row {row.RowIndex}: {ex.Message}");
                }
            }

            if (summary.Errors == 0)
            {
                await transaction.CommitAsync();
                summary.Inserted = inserted;
            }
            else
            {
                await transaction.RollbackAsync();
                summary.Inserted = 0;
                summary.Skipped = 0;
                summary.SkippedReasons.Clear();
                summary.SkippedReasons.Add("No rows were saved because one or more rows had errors. Fix the errors and try again.");
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await audit.LogAsync("BatchCreate", "batch", "Create", null, null, userId, $"Batch create completed: {summary.Inserted} inserted, {summary.Skipped} skipped, {summary.Errors} errors", summary.Errors > 0 ? "Error" : "Information", null);
        return summary;
    }

    private static bool SkipDuplicateOrInvalid(CsvRowViewModel row, ImportSummaryViewModel summary)
    {
        if (row.IsDuplicate || row.IsCrossRowDuplicate || !string.IsNullOrEmpty(row.ValidationError))
        {
            summary.Skipped++;
            summary.SkippedReasons.Add($"Row {row.RowIndex}: {(row.IsDuplicate || row.IsCrossRowDuplicate ? "Duplicate entry" : row.ValidationError)}");
            return true;
        }
        return false;
    }

    private async Task<Supplier?> ResolveSupplierAsync(CsvRowViewModel row, bool autoCreate, ImportSummaryViewModel summary)
    {
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == row.SupplierName);
        if (supplier is null)
        {
            if (autoCreate)
            {
                supplier = new Supplier
                {
                    SupplierName = row.SupplierName,
                    Location = row.Location,
                    VatNo = row.VatNo
                };
                db.Suppliers.Add(supplier);
                await db.SaveChangesAsync();
            }
            else
            {
                summary.Skipped++;
                summary.SkippedReasons.Add($"Row {row.RowIndex}: Supplier '{row.SupplierName}' not found");
                return null;
            }
        }
        return supplier;
    }

    private async Task<Item?> ResolveItemAsync(CsvRowViewModel row, bool autoCreate, ImportSummaryViewModel summary)
    {
        var item = await db.Items.FirstOrDefaultAsync(i => i.ItemName == row.ItemName);
        if (item is null)
        {
            if (autoCreate)
            {
                item = new Item { ItemName = row.ItemName };
                db.Items.Add(item);
                await db.SaveChangesAsync();
            }
            else
            {
                summary.Skipped++;
                summary.SkippedReasons.Add($"Row {row.RowIndex}: Item '{row.ItemName}' not found");
                return null;
            }
        }
        return item;
    }

    private async Task<FiscalYear?> ResolveFiscalYearAsync(CsvRowViewModel row, ImportSummaryViewModel summary)
    {
        FiscalYear? fiscalYear = null;
        if (row.FiscalYearId > 0)
            fiscalYear = await db.FiscalYears.FindAsync(row.FiscalYearId);
        else
        {
            var fyName = FiscalYearHelper.GetFiscalYearName(row.Miti);
            fiscalYear = await db.FiscalYears.FirstOrDefaultAsync(f => f.Name == fyName);
        }

        if (fiscalYear is null)
        {
            summary.Skipped++;
            summary.SkippedReasons.Add($"Row {row.RowIndex}: Fiscal year not found for Miti '{row.Miti}'");
        }
        return fiscalYear;
    }

    private static void AddWarnings(CsvRowViewModel row, ImportSummaryViewModel summary)
    {
        if (row.IsVatMismatch || row.IsTotalMismatch)
        {
            summary.WarningCount++;
            var reasons = new List<string>();
            if (row.IsVatMismatch) reasons.Add("VAT amount mismatch");
            if (row.IsTotalMismatch) reasons.Add("Total amount mismatch");
            summary.Warnings.Add($"Row {row.RowIndex}: {string.Join(", ", reasons)} (imported with server-calculated values)");
        }
    }

    private static ExpenseDetail CreateDetail(int expenseId, ExpenseDetailViewModel d)
    {
        var taxable = d.TaxableAmount > 0 ? d.TaxableAmount : d.Quantity * d.Rate;
        var vat = RoundMoney(taxable * AppConstants.VatRate);
        var total = RoundMoney(taxable + vat);

        return new ExpenseDetail
        {
            ExpenseId = expenseId,
            ItemId = d.ItemId,
            Quantity = d.Quantity,
            Rate = d.Rate,
            TaxableAmount = taxable,
            VatAmount = vat,
            TotalAmount = total
        };
    }

    private static ExpenseDetail CreateDetailFromRow(int expenseId, int itemId, CsvRowViewModel row)
    {
        var taxable = row.TaxableAmount > 0 ? row.TaxableAmount : row.Quantity * row.Rate;
        var vat = RoundMoney(taxable * AppConstants.VatRate);
        var total = RoundMoney(taxable + vat);

        return new ExpenseDetail
        {
            ExpenseId = expenseId,
            ItemId = itemId,
            Quantity = row.Quantity,
            Rate = row.Rate,
            TaxableAmount = taxable,
            VatAmount = vat,
            TotalAmount = total
        };
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string? GetSafeNepaliMonth(string miti)
    {
        var parts = miti.Split('/');
        if (parts.Length != 3) return null;
        if (!int.TryParse(NepaliDateHelper.ConvertToEnglishDigits(parts[1]), NumberStyles.None, CultureInfo.InvariantCulture, out var month))
            return null;
        if (month < 1 || month > 12) return null;
        return NepaliDateHelper.NepaliMonthNames[month - 1];
    }

    private async Task<int> GetDefaultCategoryIdAsync()
    {
        var category = await db.ExpenseCategories.AsNoTracking()
            .OrderBy(c => c.CategoryName == "Miscellaneous" ? 0 : 1)
            .ThenBy(c => c.CategoryId)
            .FirstOrDefaultAsync();
        return category?.CategoryId ?? 1;
    }
}
