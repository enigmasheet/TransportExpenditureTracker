using System.ComponentModel.DataAnnotations;

namespace TransportExpenditureTracker.Models
{
    public class Item
    {
        public Item()
        {
            Invoices = new HashSet<Invoice>();
        }

        [Key]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = null!;

        public ICollection<Invoice> Invoices { get; set; }

    }
}
