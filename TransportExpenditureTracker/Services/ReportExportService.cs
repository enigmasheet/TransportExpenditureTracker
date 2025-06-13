using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services
{
    public class ReportExportService : IReportExportService
    {
        public byte[] GenerateVatInvoiceExcel(List<ReportRowViewModel> reports, Company company)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("VAT Invoice Report");

            // Merge and set header info across columns 1-12
            worksheet.Range(1, 1, 1, 12).Merge().Value = $"Company: {company.Name}";
            worksheet.Range(1, 1, 1, 12).Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            worksheet.Range(2, 1, 2, 12).Merge().Value = $"Location: {company.Location}";
            worksheet.Range(3, 1, 3, 12).Merge().Value = $"VAT No: {company.VatNumber}";
            worksheet.Range(4, 1, 4, 12).Merge().Value = $"Contact: {company.ContactNumber}";

            // Report Title - merged and centered
            worksheet.Range(6, 1, 6, 12).Merge().Value = "VAT Invoice Report";
            worksheet.Range(6, 1, 6, 12).Style.Font.Bold = true;
            worksheet.Range(6, 1, 6, 12).Style.Font.FontSize = 16;
            worksheet.Range(6, 1, 6, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Column Headers
            var headers = new[]
            {
            "S.N.", "Date", "Invoice No", "Party", "Location", "Vat No",
            "Item", "Qty", "Rate", "Taxable", "VAT", "Total Amount"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(8, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data Rows
            int row = 9;
            decimal totalTaxable = 0, totalVat = 0, totalAmount = 0;

            foreach (var item in reports)
            {
                worksheet.Cell(row, 1).Value = item.Sno;
                worksheet.Cell(row, 2).Value = item.Miti;
                worksheet.Cell(row, 3).Value = item.InvoiceNo;
                worksheet.Cell(row, 4).Value = item.PartyName;
                worksheet.Cell(row, 5).Value = item.Location;
                worksheet.Cell(row, 6).Value = item.VatNo;
                worksheet.Cell(row, 7).Value = item.ItemName;
                worksheet.Cell(row, 8).Value = item.Quantity;
                worksheet.Cell(row, 9).Value = item.Rate;
                worksheet.Cell(row, 10).Value = item.TaxableAmount;
                worksheet.Cell(row, 11).Value = item.VatAmount;
                worksheet.Cell(row, 12).Value = item.TotalAmount;

                totalTaxable += item.TaxableAmount;
                totalVat += item.VatAmount;
                totalAmount += item.TotalAmount;

                row++;
            }

            // Totals row styling
            var totalLabelCell = worksheet.Cell(row, 9);
            totalLabelCell.Value = "Total:";
            totalLabelCell.Style.Font.Bold = true;
            totalLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            totalLabelCell.Style.Alignment.Indent = 1;

            worksheet.Cell(row, 10).Value = totalTaxable;
            worksheet.Cell(row, 11).Value = totalVat;
            worksheet.Cell(row, 12).Value = totalAmount;
            worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;

            // Format columns
            worksheet.Column(8).Style.NumberFormat.Format = "#,##0";      // Qty as integer
            worksheet.Column(9).Style.NumberFormat.Format = "#,##0.00";   // Rate
            worksheet.Column(10).Style.NumberFormat.Format = "#,##0.00";  // Taxable
            worksheet.Column(11).Style.NumberFormat.Format = "#,##0.00";  // VAT
            worksheet.Column(12).Style.NumberFormat.Format = "#,##0.00";  // Total Amount

            // Align text columns left
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Align numeric columns right
            for (int i = 8; i <= 12; i++)
                worksheet.Column(i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


            // Add autofilter to header row
            worksheet.Range(8, 1, row - 1, 12).SetAutoFilter();

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
     



        public byte[] GenerateVatInvoiceReport(List<ReportRowViewModel> data, Company company)
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
                var companyDetails = new Paragraph($"{company.Name}\nLocation: {company.Location}\nVAT No: {company.VatNumber}\nContact: {company.ContactNumber}", companyFont)
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

                string text = $"Page {writer.PageNumber} - This is a computer generated report";
                footerTbl.AddCell(new Phrase(text, footerFont));

                footerTbl.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 5, writer.DirectContent);
            }
        }
    }
}
