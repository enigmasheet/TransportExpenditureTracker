using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models
{
    public class UserCompany
    {
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;

        [Required]
        public int CompanyId { get; set; }

        public Company? Company { get; set; }
    }

}
