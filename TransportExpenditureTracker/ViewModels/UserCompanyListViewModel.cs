namespace TransportExpenditureTracker.ViewModels
{
    public class UserCompanyListViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<string> CompanyNames { get; set; } = new List<string>();
    }

}
