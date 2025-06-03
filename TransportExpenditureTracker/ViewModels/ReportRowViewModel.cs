namespace TransportExpenditureTracker.ViewModels
{
    public class ReportRowViewModel
    {
        public int Sno { get; set; }
        public string Miti { get; set; } = null!;
        public string InvoiceNo { get; set; } = null!;
        public string PartyName { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string VatNo { get; set; } = null!;
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount => TaxableAmount + VatAmount;
    }
}
