using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;

    public ReportService(ApplicationDbContext db)
    {
        _db = db;
    }

    private IQueryable<ExpenseHeader> ApplyHeaderFilters(IQueryable<ExpenseHeader> query, ReportFilterViewModel filters)
    {
        if (!string.IsNullOrEmpty(filters.FiscalYear))
            query = query.Where(h => h.FiscalYear == filters.FiscalYear);
        if (!string.IsNullOrEmpty(filters.NepaliMonth))
            query = query.Where(h => h.NepaliMonth == filters.NepaliMonth);
        if (filters.FromDate.HasValue)
            query = query.Where(h => h.EnglishDate >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(h => h.EnglishDate <= filters.ToDate.Value);
        if (filters.SupplierId.HasValue)
            query = query.Where(h => h.SupplierId == filters.SupplierId.Value);
        if (filters.CategoryId.HasValue)
            query = query.Where(h => h.CategoryId == filters.CategoryId.Value);
        if (!string.IsNullOrEmpty(filters.InvoiceNo))
            query = query.Where(h => h.InvoiceNo.Contains(filters.InvoiceNo));
        if (!string.IsNullOrEmpty(filters.PaymentMethod))
            query = query.Where(h => h.PaymentMethod == filters.PaymentMethod);
        return query;
    }

    private IQueryable<ReportRowViewModel> GetBaseQuery(ReportFilterViewModel filters)
    {
        var query = from h in _db.ExpenseHeaders.AsNoTracking()
                    join d in _db.ExpenseDetails.AsNoTracking() on h.ExpenseId equals d.ExpenseId
                    join s in _db.Suppliers.AsNoTracking() on h.SupplierId equals s.SupplierId
                    join c in _db.ExpenseCategories.AsNoTracking() on h.CategoryId equals c.CategoryId
                    join i in _db.Items.AsNoTracking() on d.ItemId equals i.ItemId
                    select new ReportRowViewModel
                    {
                        Miti = h.Miti,
                        InvoiceNo = h.InvoiceNo,
                        FiscalYear = h.FiscalYear,
                        NepaliMonth = h.NepaliMonth,
                        EnglishDate = h.EnglishDate,
                        SupplierName = s.SupplierName,
                        Location = s.Location ?? "",
                        VatNo = s.VatNo ?? "",
                        CategoryName = c.CategoryName,
                        ItemName = i.ItemName,
                        Unit = i.Unit ?? "",
                        PaymentMethod = h.PaymentMethod,
                        Quantity = d.Quantity,
                        Rate = d.Rate,
                        TaxableAmount = d.TaxableAmount,
                        VatAmount = d.VatAmount,
                        TotalAmount = d.TotalAmount
                    };

        if (!string.IsNullOrEmpty(filters.FiscalYear))
            query = query.Where(r => r.FiscalYear == filters.FiscalYear);
        if (!string.IsNullOrEmpty(filters.NepaliMonth))
            query = query.Where(r => r.NepaliMonth == filters.NepaliMonth);
        if (filters.FromDate.HasValue)
            query = query.Where(r => r.EnglishDate >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(r => r.EnglishDate <= filters.ToDate.Value);
        if (filters.SupplierId.HasValue)
            query = query.Where(r => r.SupplierName != null); // supplier filter applied via headers
        if (filters.ItemId.HasValue)
            query = query.Where(r => r.ItemName != null); // item filter applied via join
        if (!string.IsNullOrEmpty(filters.InvoiceNo))
            query = query.Where(r => r.InvoiceNo.Contains(filters.InvoiceNo));
        if (!string.IsNullOrEmpty(filters.PaymentMethod))
            query = query.Where(r => r.PaymentMethod == filters.PaymentMethod);
        if (!string.IsNullOrEmpty(filters.Location))
            query = query.Where(r => r.Location.Contains(filters.Location));

        return query;
    }

    private IQueryable<ReportRowViewModel> GetBaseQueryWithDetailFilters(ReportFilterViewModel filters)
    {
        var query = from h in _db.ExpenseHeaders.AsNoTracking()
                    join d in _db.ExpenseDetails.AsNoTracking() on h.ExpenseId equals d.ExpenseId
                    join s in _db.Suppliers.AsNoTracking() on h.SupplierId equals s.SupplierId
                    join c in _db.ExpenseCategories.AsNoTracking() on h.CategoryId equals c.CategoryId
                    join i in _db.Items.AsNoTracking() on d.ItemId equals i.ItemId
                    select new ReportRowViewModel
                    {
                        Miti = h.Miti,
                        InvoiceNo = h.InvoiceNo,
                        FiscalYear = h.FiscalYear,
                        NepaliMonth = h.NepaliMonth,
                        EnglishDate = h.EnglishDate,
                        SupplierName = s.SupplierName,
                        Location = s.Location ?? "",
                        VatNo = s.VatNo ?? "",
                        CategoryName = c.CategoryName,
                        ItemName = i.ItemName,
                        Unit = i.Unit ?? "",
                        PaymentMethod = h.PaymentMethod,
                        Quantity = d.Quantity,
                        Rate = d.Rate,
                        TaxableAmount = d.TaxableAmount,
                        VatAmount = d.VatAmount,
                        TotalAmount = d.TotalAmount,
                        SupplierId = h.SupplierId,
                        CategoryId = h.CategoryId,
                        ItemId = d.ItemId
                    };

        if (!string.IsNullOrEmpty(filters.FiscalYear))
            query = query.Where(r => r.FiscalYear == filters.FiscalYear);
        if (!string.IsNullOrEmpty(filters.NepaliMonth))
            query = query.Where(r => r.NepaliMonth == filters.NepaliMonth);
        if (filters.FromDate.HasValue)
            query = query.Where(r => r.EnglishDate >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(r => r.EnglishDate <= filters.ToDate.Value);
        if (filters.SupplierId.HasValue)
            query = query.Where(r => r.SupplierId == filters.SupplierId.Value);
        if (filters.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == filters.CategoryId.Value);
        if (filters.ItemId.HasValue)
            query = query.Where(r => r.ItemId == filters.ItemId.Value);
        if (!string.IsNullOrEmpty(filters.InvoiceNo))
            query = query.Where(r => r.InvoiceNo.Contains(filters.InvoiceNo));
        if (!string.IsNullOrEmpty(filters.PaymentMethod))
            query = query.Where(r => r.PaymentMethod == filters.PaymentMethod);
        if (!string.IsNullOrEmpty(filters.Location))
            query = query.Where(r => r.Location.Contains(filters.Location));
        if (!string.IsNullOrEmpty(filters.VatNo))
            query = query.Where(r => r.VatNo.Contains(filters.VatNo));

        return query;
    }

    private async Task<List<ReportRowViewModel>> GetPagedAsync(IQueryable<ReportRowViewModel> query, ReportFilterViewModel filters)
    {
        var skip = (filters.PageNumber - 1) * filters.PageSize;
        var items = await query.Skip(skip).Take(filters.PageSize).ToListAsync();

        int idx = 0;
        foreach (var item in items)
        {
            item.Sno = idx + 1 + skip;
            idx++;
        }

        return items;
    }

    public async Task<List<ReportRowViewModel>> GetDailyReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.EnglishDate).ThenBy(r => r.InvoiceNo);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetMonthlyReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.NepaliMonth).ThenBy(r => r.EnglishDate).ThenBy(r => r.InvoiceNo);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetFiscalYearReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.EnglishDate).ThenBy(r => r.InvoiceNo);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetSupplierWiseReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.SupplierName).ThenBy(r => r.EnglishDate);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetCategoryWiseReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.CategoryName).ThenBy(r => r.EnglishDate);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetItemWiseReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.ItemName).ThenBy(r => r.EnglishDate);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetVatPaidReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).Where(r => r.VatAmount > 0).OrderByDescending(r => r.VatAmount);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetPaymentMethodReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.PaymentMethod).ThenBy(r => r.EnglishDate);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetLocationWiseReportAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.Location).ThenBy(r => r.EnglishDate);
        return await GetPagedAsync(query, filters);
    }

    public async Task<List<ReportRowViewModel>> GetDetailedLedgerAsync(ReportFilterViewModel filters)
    {
        var query = GetBaseQueryWithDetailFilters(filters).OrderBy(r => r.EnglishDate).ThenBy(r => r.InvoiceNo);
        return await GetPagedAsync(query, filters);
    }

    public async Task<int> GetTotalCountAsync(ReportFilterViewModel filters, string reportType)
    {
        var query = GetBaseQueryWithDetailFilters(filters);
        return await query.CountAsync();
    }
}
