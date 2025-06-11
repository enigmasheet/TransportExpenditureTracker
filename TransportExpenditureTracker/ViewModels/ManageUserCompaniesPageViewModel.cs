namespace TransportExpenditureTracker.ViewModels
{
    public class ManageUserCompaniesPageViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<ManageUserCompaniesViewModel> Companies { get; set; }
    }

}
