namespace TransportExpenditureTracker.ViewModels;

public class ImportSummaryViewModel
{
    public int Inserted { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = [];
    public List<string> SkippedReasons { get; set; } = [];
}
