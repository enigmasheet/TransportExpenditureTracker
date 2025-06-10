using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models
{
    public class UserCompany
    {
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public IdentityUser User { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [Required]
        public int CompanyId { get; set; }

        public Company? Company { get; set; }
    }

}
