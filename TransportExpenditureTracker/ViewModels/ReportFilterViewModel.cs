using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels
{
    public class ReportFilterViewModel
    {
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        [Required(ErrorMessage = "Fiscal Year is required.")]
        [Display(Name = "Fiscal Year")]
        public string FiscalYear { get; set; }

        [Display(Name = "Fiscal Month")]
        public string? FiscalMonth { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNo { get; set; }

        [Display(Name = "Item")]
        public int? ItemId { get; set; }

        [Display(Name = "Party")]
        public int? PartyId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }
}
