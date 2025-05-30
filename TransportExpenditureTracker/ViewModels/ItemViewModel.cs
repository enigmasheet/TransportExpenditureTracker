using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TransportExpenditureTracker.ViewModels
{
    public class ItemViewModel
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        // Optional: List of Invoices this item is associated with
        public List<SelectListItem>? InvoiceOptions { get; set; }

        // Optional: Use this if you want to bind selected invoices from the form
       // public List<int>? SelectedInvoiceIds { get; set; }
    }
}
