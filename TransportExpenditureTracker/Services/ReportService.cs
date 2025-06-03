// Services/ReportService.cs
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReportRowViewModel>> GetVatInvoiceReportAsync(ReportFilterViewModel filters)
        {
            var query = _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.Item)
                .AsQueryable();

            // Apply filters
            if (filters.FromDate.HasValue)
                query = query.Where(i => i.Miti >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                query = query.Where(i => i.Miti <= filters.ToDate.Value);

            if (filters.PartyId.HasValue)
                query = query.Where(i => i.PartyId == filters.PartyId.Value);

            if (!string.IsNullOrEmpty(filters.FiscalYear))
                query = query.Where(i => i.FiscalYear == filters.FiscalYear);
            
            if (!string.IsNullOrEmpty(filters.FiscalMonth))
                query = query.Where(i => i.FiscalMonth == filters.FiscalMonth);

            if (!string.IsNullOrEmpty(filters.InvoiceNo))
                query = query.Where(i => i.InvoiceNo.Contains(filters.InvoiceNo));

            if (filters.ItemId.HasValue)  // Changed from ItemName to ItemId filtering
                query = query.Where(i => i.ItemId == filters.ItemId.Value);

            var invoices = await query.OrderBy(i => i.Miti).ToListAsync();

            var result = invoices.Select((invoice, index) => new ReportRowViewModel
            {
                Sno = index + 1,
                Miti = invoice.Miti.ToString("yyyy-MM-dd"),
                InvoiceNo = invoice.InvoiceNo,
                PartyName = invoice.Party.PartyName,
                Location = invoice.Party.Location ?? "",
                VatNo = invoice.Party.VatNo ?? "",
                ItemName = invoice.Item.ItemName,
                Quantity = invoice.Quantity,
                Rate = invoice.Rate ?? 0,
                TaxableAmount = invoice.TaxableAmount,
                VatAmount = invoice.VatAmount,
            }).ToList();

            return result;
        }


    }
}
