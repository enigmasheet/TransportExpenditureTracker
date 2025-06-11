namespace TransportExpenditureTracker.ViewModels
{ 
    public class UserRolesViewModel
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<string> CompanyNames { get; set; } = [];

    }

}
