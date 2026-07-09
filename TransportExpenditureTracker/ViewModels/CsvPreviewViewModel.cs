namespace TransportExpenditureTracker.ViewModels;

public class CsvPreviewViewModel
{
    public List<CsvRowViewModel> Rows { get; set; } = [];
    public int TotalRows { get; set; }
    public int DuplicateCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
