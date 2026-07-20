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

    public async Task AddAsync(ExpenseEntryViewModel vm, string userId)
    {
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var header = new ExpenseHeader
            {
                InvoiceNo = vm.InvoiceNo,
                Miti = vm.Miti,
                EnglishDate = NepaliDateHelper.ParseNepaliDate(vm.Miti) ?? DateTime.UtcNow,
                NepaliMonth = vm.NepaliMonth,
                SupplierId = vm.SupplierId,
                CategoryId = vm.CategoryId,
                FiscalYearId = vm.FiscalYearId,
                PaymentMethod = vm.PaymentMethod ?? string.Empty,
                Remarks = vm.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            db.ExpenseHeaders.Add(header);
            await db.SaveChangesAsync();

            if (vm.Details is not null)
            {
                foreach (var d in vm.Details)
                {
                    var detail = CreateDetail(header.ExpenseId, d);
                    db.ExpenseDetails.Add(detail);
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            await audit.LogAsync("ExpenseHeader", header.ExpenseId.ToString(CultureInfo.InvariantCulture), "Create", null, JsonSerializer.Serialize(vm), userId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(ExpenseEntryViewModel vm, string userId)
    {
        var existing = await db.ExpenseHeaders
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.ExpenseId == vm.ExpenseId);

        if (existing is null) return;

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
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var existing = await db.ExpenseHeaders
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.ExpenseId == id);

        if (existing is null) return;

        var oldValues = JsonSerializer.Serialize(converter.ToEntryViewModel(existing));

        db.ExpenseHeaders.Remove(existing);
        await db.SaveChangesAsync();

        await audit.LogAsync("ExpenseHeader", id.ToString(CultureInfo.InvariantCulture), "Delete", oldValues, null, userId);
    }

    public async Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId, int fiscalYearId, int? excludeId = null)
    {
        var query = db.ExpenseHeaders.AsNoTracking()
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

                var header = new ExpenseHeader
                {
                    InvoiceNo = row.InvoiceNo,
                    Miti = row.Miti,
                    EnglishDate = englishDate,
                    FiscalYearId = fiscalYear.Id,
                    SupplierId = supplier.SupplierId,
                    CategoryId = 1,
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

        return summary;
    }

    public async Task<ImportSummaryViewModel> BatchCreateAsync(ExpenseBatchViewModel vm, string userId)
    {
        using var transaction = await db.Database.BeginTransactionAsync();
        var summary = new ImportSummaryViewModel();

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

                    var mitiParts = row.Miti.Split('/');
                    var monthIndex = int.Parse(NepaliDateHelper.ConvertToEnglishDigits(mitiParts[1]), CultureInfo.InvariantCulture) - 1;
                    var nepaliMonth = NepaliDateHelper.NepaliMonthNames[monthIndex];

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

                    var taxableAmount = Math.Round(row.Quantity * row.Rate, 2);
                    var vatAmount = Math.Round(taxableAmount * AppConstants.VatRate, 2);
                    var totalAmount = Math.Round(taxableAmount + vatAmount, 2);

                    var header = new ExpenseHeader
                    {
                        InvoiceNo = row.InvoiceNo,
                        Miti = row.Miti,
                        EnglishDate = englishDate,
                        FiscalYearId = fiscalYear.Id,
                        NepaliMonth = nepaliMonth,
                        SupplierId = row.SupplierId,
                        CategoryId = row.CategoryId ?? 1,
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

                    summary.Inserted++;
                }
                catch (Exception ex)
                {
                    summary.Errors++;
                    summary.ErrorMessages.Add($"Row {row.RowIndex}: {ex.Message}");
                }
            }

            if (summary.Errors == 0)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

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
        var vat = Math.Round(taxable * AppConstants.VatRate, 2);
        var total = Math.Round(taxable + vat, 2);

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
        var vat = Math.Round(taxable * AppConstants.VatRate, 2);
        var total = Math.Round(taxable + vat, 2);

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
}
