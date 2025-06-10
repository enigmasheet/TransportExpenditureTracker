using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        [Display(Name = "VAT Number")]
        public required string VatNumber { get; set; }

        public ICollection<UserCompany>? UserCompanies { get; set; }
    }

}
