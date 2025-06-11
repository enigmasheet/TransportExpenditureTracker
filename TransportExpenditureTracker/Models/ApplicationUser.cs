
using Microsoft.AspNetCore.Identity;

namespace TransportExpenditureTracker.Models

{

    public class ApplicationUser : IdentityUser
    {
        public UserCompany UserCompany { get; set; } = null!;
    }

}
