namespace TransportExpenditureTracker.ViewModels;

public class ModalFormConfig
{
    public string ModalId { get; set; } = string.Empty;
    public string FormId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SubmitText { get; set; } = "Save";
    public string SubmitIcon { get; set; } = "bi-floppy";
    public string SubmitClass { get; set; } = "btn-primary";
    public string DialogClass { get; set; } = string.Empty;
    public bool IncludeAntiforgery { get; set; } = true;
    public Func<object, object>? BodyContent { get; set; }
}