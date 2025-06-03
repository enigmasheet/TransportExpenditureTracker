namespace TransportExpenditureTracker.ViewModels
{
    // ViewModels/ReportFilterViewModel.cs
    public class ReportFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FiscalYear { get; set; }
        public string? FiscalMonth { get; set; } // optional if you want to filter by month
        public string InvoiceNo { get; set; }
        public int? ItemId { get; set; }  // <-- Changed from ItemName to ItemId
        public int? PartyId { get; set; } // optional if you want to filter by party
    }

}
