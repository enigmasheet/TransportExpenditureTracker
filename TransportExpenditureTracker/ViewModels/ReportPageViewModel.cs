namespace TransportExpenditureTracker.ViewModels;

public class ReportPageViewModel
{
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public List<ReportRowViewModel> Rows { get; set; } = new();
    public ReportFilterViewModel? Filters { get; set; }
}
