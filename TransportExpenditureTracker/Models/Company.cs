using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace TransportExpenditureTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        [Display(Name = "VAT Number")]
        [MaxLength(9, ErrorMessage = "VAT number cannot exceed 9 characters.")]
        public required string VatNumber { get; set; }
       
        [Required]
        public required string Location { get; set; }
        
        [Required]
        [Display(Name ="Contact Number")]
        [MaxLength(15, ErrorMessage = "Contact number cannot exceed 15 characters.")]
        public required string ContactNumber { get; set; }

        public ICollection<UserCompany> UserCompanies { get; set; } = [];
    }

}
