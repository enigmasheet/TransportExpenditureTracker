using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _db;
    private readonly ExpenseConverter _converter;
    private readonly IAuditService _audit;

    public ExpenseService(ApplicationDbContext db, ExpenseConverter converter, IAuditService audit)
    {
        _db = db;
        _converter = converter;
        _audit = audit;
    }

    public async Task<List<ExpenseHeaderViewModel>> GetAllAsync()
    {
        var headers = await _db.ExpenseHeaders
            .Include(h => h.Supplier)
            .Include(h => h.Category)
            .Include(h => h.Details)
            .ThenInclude(d => d.Item)
            .OrderByDescending(h => h.EnglishDate)
            .ToListAsync();

        return headers.Select(_converter.ToHeaderViewModel).ToList();
    }

    public async Task<ExpenseEntryViewModel?> GetByIdAsync(int id)
    {
        var header = await _db.ExpenseHeaders
            .Include(h => h.Supplier)
            .Include(h => h.Category)
            .Include(h => h.Details)
            .ThenInclude(d => d.Item)
            .FirstOrDefaultAsync(h => h.ExpenseId == id);

        return header is null ? null : _converter.ToEntryViewModel(header);
    }

    public async Task AddAsync(ExpenseEntryViewModel vm, string userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var header = new ExpenseHeader
            {
                InvoiceNo = vm.InvoiceNo,
                Miti = vm.Miti,
                EnglishDate = NepaliDateHelper.ParseNepaliDate(vm.Miti) ?? DateTime.UtcNow,
                FiscalYear = vm.FiscalYear ?? string.Empty,
                NepaliMonth = vm.NepaliMonth,
                SupplierId = vm.SupplierId,
                CategoryId = vm.CategoryId,
                PaymentMethod = vm.PaymentMethod ?? string.Empty,
                Remarks = vm.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _db.ExpenseHeaders.Add(header);
            await _db.SaveChangesAsync();

            if (vm.Details is not null)
            {
                foreach (var d in vm.Details)
                {
                    var detail = new ExpenseDetail
                    {
                        ExpenseId = header.ExpenseId,
                        ItemId = d.ItemId,
                        Quantity = d.Quantity,
                        Rate = d.Rate,
                        TaxableAmount = d.TaxableAmount,
                        VatAmount = d.VatAmount,
                        TotalAmount = d.TotalAmount
                    };
                    _db.ExpenseDetails.Add(detail);
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _audit.LogAsync("ExpenseHeader", header.ExpenseId.ToString(), "Create", null, JsonSerializer.Serialize(vm), userId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(ExpenseEntryViewModel vm, string userId)
    {
        var existing = await _db.ExpenseHeaders
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.ExpenseId == vm.ExpenseId);

        if (existing is null) return;

        var oldValues = JsonSerializer.Serialize(_converter.ToEntryViewModel(existing));

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            existing.InvoiceNo = vm.InvoiceNo;
            existing.Miti = vm.Miti;
            existing.EnglishDate = NepaliDateHelper.ParseNepaliDate(vm.Miti) ?? existing.EnglishDate;
            existing.FiscalYear = vm.FiscalYear ?? string.Empty;
            existing.NepaliMonth = vm.NepaliMonth;
            existing.SupplierId = vm.SupplierId;
            existing.CategoryId = vm.CategoryId;
            existing.PaymentMethod = vm.PaymentMethod ?? string.Empty;
            existing.Remarks = vm.Remarks;
            existing.UpdatedAt = DateTime.UtcNow;

            _db.ExpenseDetails.RemoveRange(existing.Details);
            await _db.SaveChangesAsync();

            if (vm.Details is not null)
            {
                foreach (var d in vm.Details)
                {
                    var detail = new ExpenseDetail
                    {
                        ExpenseId = existing.ExpenseId,
                        ItemId = d.ItemId,
                        Quantity = d.Quantity,
                        Rate = d.Rate,
                        TaxableAmount = d.TaxableAmount,
                        VatAmount = d.VatAmount,
                        TotalAmount = d.TotalAmount
                    };
                    _db.ExpenseDetails.Add(detail);
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            var newValues = JsonSerializer.Serialize(vm);
            await _audit.LogAsync("ExpenseHeader", existing.ExpenseId.ToString(), "Update", oldValues, newValues, userId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var existing = await _db.ExpenseHeaders
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.ExpenseId == id);

        if (existing is null) return;

        var oldValues = JsonSerializer.Serialize(_converter.ToEntryViewModel(existing));

        _db.ExpenseHeaders.Remove(existing);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("ExpenseHeader", id.ToString(), "Delete", oldValues, null, userId);
    }

    public async Task<bool> IsDuplicateInvoiceAsync(string invoiceNo, int supplierId)
    {
        return await _db.ExpenseHeaders.AnyAsync(h => h.InvoiceNo == invoiceNo && h.SupplierId == supplierId);
    }

    public async Task<ImportSummaryViewModel> ImportCsvAsync(List<CsvRowViewModel> rows, string userId, bool autoCreate)
    {
        var summary = new ImportSummaryViewModel();

        foreach (var row in rows)
        {
            try
            {
                if (row.IsDuplicate || !string.IsNullOrEmpty(row.ValidationError))
                {
                    summary.Skipped++;
                    summary.SkippedReasons.Add($"Row {row.RowIndex}: {(row.IsDuplicate ? "Duplicate entry" : row.ValidationError)}");
                    continue;
                }

                Supplier? supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == row.SupplierName);
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
                        _db.Suppliers.Add(supplier);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Supplier '{row.SupplierName}' not found");
                        continue;
                    }
                }

                Item? item = await _db.Items.FirstOrDefaultAsync(i => i.ItemName == row.ItemName);
                if (item is null)
                {
                    if (autoCreate)
                    {
                        item = new Item { ItemName = row.ItemName };
                        _db.Items.Add(item);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        summary.Skipped++;
                        summary.SkippedReasons.Add($"Row {row.RowIndex}: Item '{row.ItemName}' not found");
                        continue;
                    }
                }

                var englishDate = NepaliDateHelper.ParseNepaliDate(row.Miti) ?? DateTime.UtcNow;

                var header = new ExpenseHeader
                {
                    InvoiceNo = row.InvoiceNo,
                    Miti = row.Miti,
                    EnglishDate = englishDate,
                    FiscalYear = englishDate.Month >= 4 ? englishDate.Year.ToString() : (englishDate.Year - 1).ToString(),
                    SupplierId = supplier.SupplierId,
                    CategoryId = 1,
                    PaymentMethod = "Cash",
                    CreatedAt = DateTime.UtcNow
                };
                _db.ExpenseHeaders.Add(header);
                await _db.SaveChangesAsync();

                var detail = new ExpenseDetail
                {
                    ExpenseId = header.ExpenseId,
                    ItemId = item.ItemId,
                    Quantity = row.Quantity,
                    Rate = row.Rate,
                    TaxableAmount = row.TaxableAmount,
                    VatAmount = row.VatAmount,
                    TotalAmount = row.TotalAmount
                };
                _db.ExpenseDetails.Add(detail);
                await _db.SaveChangesAsync();

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
}
