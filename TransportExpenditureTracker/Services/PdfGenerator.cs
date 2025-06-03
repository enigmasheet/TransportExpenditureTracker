using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using TransportExpenditureTracker.ViewModels;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        public byte[] GenerateVatInvoiceReport(List<ReportRowViewModel> data)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 20, 20, 20, 20);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                writer.PageEvent = new PdfFooter();

                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                // Company details
                Paragraph companyDetails = new Paragraph("A.B Carriers\nLocation: Birgunj\nVAT No: 305314651\nContact: 9855026016", companyFont)
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingAfter = 10f
                };
                document.Add(companyDetails);

                // Report title
                document.Add(new Paragraph("VAT Invoice Report", titleFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(12)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] {
                        1f,    // S.N.
                        3.5f,    // Date
                        2.3f,    // Invoice No
                        4f,    // Party (wider)
                        3f,    // Location (wider)
                        3.3f,  // Vat No
                        2f,    // Item
                        1.5f,  // Qty
                        2.5f,  // Rate
                        3f,    // Taxable
                        2.5f,    // VAT
                        3f     // Total Amount
                    });

                // Helper method to add cell with padding and alignment
                void AddCell(PdfPTable tbl, string text, Font font, int horizontalAlignment = Element.ALIGN_LEFT, BaseColor bgColor = null, int colspan = 1)
                {
                    var cell = new PdfPCell(new Phrase(text, font))
                    {
                       
                        HorizontalAlignment = horizontalAlignment,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = bgColor ?? BaseColor.White,
                        Colspan = colspan,
                        BorderWidth = 0.5f,
                        NoWrap = false 


                    };
                    tbl.AddCell(cell);
                }

                // Add header row with padding and gray background
                string[] headers = { "S.N.", "Date", "Invoice No", "Party", "Location", "Vat No", "Item", "Qty", "Rate", "Taxable", "VAT", "Total Amount" };
                foreach (var header in headers)
                {
                    AddCell(table, header, headerFont, Element.ALIGN_CENTER, BaseColor.LightGray);
                }

                // Data rows
                decimal totalTaxable = 0, totalVat = 0, totalAmount = 0;

                foreach (var item in data)
                {
                    AddCell(table, item.Sno.ToString(), bodyFont, Element.ALIGN_CENTER);
                    AddCell(table, item.Miti, bodyFont);
                    AddCell(table, item.InvoiceNo, bodyFont);
                    AddCell(table, item.PartyName, bodyFont);
                    AddCell(table, item.Location, bodyFont);
                    AddCell(table, item.VatNo, bodyFont);
                    AddCell(table, item.ItemName, bodyFont);
                    AddCell(table, item.Quantity.ToString(), bodyFont, Element.ALIGN_CENTER);
                    AddCell(table, item.Rate.ToString("N2"), bodyFont, Element.ALIGN_RIGHT);
                    AddCell(table, item.TaxableAmount.ToString("N2"), bodyFont, Element.ALIGN_RIGHT);
                    AddCell(table, item.VatAmount.ToString("N2"), bodyFont, Element.ALIGN_RIGHT);
                    AddCell(table, item.TotalAmount.ToString("N2"), bodyFont, Element.ALIGN_RIGHT);

                    totalTaxable += item.TaxableAmount;
                    totalVat += item.VatAmount;
                    totalAmount += item.TotalAmount;
                }

                // Total Row
                AddCell(table, "Total", headerFont, Element.ALIGN_RIGHT, BaseColor.LightGray, 9);
                AddCell(table, totalTaxable.ToString("N2"), headerFont, Element.ALIGN_RIGHT, BaseColor.LightGray);
                AddCell(table, totalVat.ToString("N2"), headerFont, Element.ALIGN_RIGHT, BaseColor.LightGray);
                AddCell(table, totalAmount.ToString("N2"), headerFont, Element.ALIGN_RIGHT, BaseColor.LightGray);

                document.Add(table);
                document.Close();

                return memoryStream.ToArray();
            }
        }


        // Class to add footer on each page
        private class PdfFooter : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.Gray);
                PdfPTable footerTbl = new PdfPTable(1)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin
                };
                footerTbl.DefaultCell.Border = Rectangle.NO_BORDER;
                footerTbl.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                footerTbl.AddCell(new Phrase("This is a computer generated report", footerFont));

                // Write footer at bottom
                footerTbl.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 5, writer.DirectContent);
            }
        }
    }
}
