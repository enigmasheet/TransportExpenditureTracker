namespace TransportExpenditureTracker.ViewModels
{
    public class ReportPageViewModel
    {
        public List<ReportRowViewModel> Reports { get; set; } = new();
        public ReportFilterViewModel Filters { get; set; } = new();
        public List<PartyViewModel> Parties { get; set; } = new();

        // For pagination
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public int PageNumber => Filters.PageNumber;
        public int PageSize => Filters.PageSize;

        public bool HasResults => Reports != null && Reports.Any();
    }
}
