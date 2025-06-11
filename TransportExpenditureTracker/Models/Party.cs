using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models
{
    public class Party
    {
        public Party()
        {
            Invoices = new HashSet<Invoice>();
        }

        [Key]
        public int PartyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartyName { get; set; } = null!;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? VatNo { get; set; }

        public int CompanyId { get; set; }

        // Navigation property for Company
        public Company? Company { get; set; }

        // Navigation property: One party can have many invoices
        public ICollection<Invoice> Invoices { get; set; }
    }

}
