namespace TransportExpenditureTracker.ViewModels
{
    public class ReportPageViewModel
    {
        public List<ReportRowViewModel> Reports { get; set; } = new();
        public ReportFilterViewModel Filters { get; set; } = new();
        public List<PartyViewModel> Parties { get; set; } = new();
    }
}
