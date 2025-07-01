namespace TransportExpenditureTracker.Services
{
    public class AuthMessageSenderOptions
    {
        public string? SendGridKey { get; set; }
        public string SenderEmail { get; set; } = "info@abhaymandal.com.np";
        public string SenderName { get; set; } = "TMS & LMS Support Team";
    }
}
