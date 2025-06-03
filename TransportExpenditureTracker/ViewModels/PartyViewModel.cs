using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.ViewModels
{
    public class PartyViewModel
    {
        public int PartyId { get; set; }

        [Required]
        [Display(Name = "Party Name")]
        public string PartyName { get; set; }= null!;

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "VAT Number")]
        public  string? VatNo { get; set; }
    }
}
