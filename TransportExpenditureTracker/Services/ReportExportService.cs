using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;
using System.Text;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services;

public class ReportExportService : IReportExportService
{
    public byte[] GenerateExcel(List<ReportRowViewModel> data, string title)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(title.Length > 31 ? title[..31] : title);

        ws.Cell(1, 1).Value = title;
        ws.Range(1, 1, 1, 10).Merge().Style.Font.Bold = true;

        var headers = new[] { "S.No", "Miti", "Invoice No", "Supplier", "Item", "Category", "Quantity", "Rate", "Taxable Amount", "VAT Amount", "Total Amount" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(3, i + 1).Value = headers[i];
            ws.Cell(3, i + 1).Style.Font.Bold = true;
            ws.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 4;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.Sno;
            ws.Cell(row, 2).Value = item.Miti;
            ws.Cell(row, 3).Value = item.InvoiceNo;
            ws.Cell(row, 4).Value = item.SupplierName;
            ws.Cell(row, 5).Value = item.ItemName;
            ws.Cell(row, 6).Value = item.CategoryName;
            ws.Cell(row, 7).Value = item.Quantity;
            ws.Cell(row, 8).Value = item.Rate;
            ws.Cell(row, 9).Value = item.TaxableAmount;
            ws.Cell(row, 10).Value = item.VatAmount;
            ws.Cell(row, 11).Value = item.TotalAmount;
            row++;
        }

        ws.Cell(row, 1).Value = "Total";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 9).FormulaA1 = $"=SUM(I4:I{row - 1})";
        ws.Cell(row, 10).FormulaA1 = $"=SUM(J4:J{row - 1})";
        ws.Cell(row, 11).FormulaA1 = $"=SUM(K4:K{row - 1})";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] GeneratePdf(List<ReportRowViewModel> data, string title)
    {
        try
        {
            FontFactory.RegisterDirectories();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Font registration failed (non-critical): {ex.Message}");
        }

        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 20f);
        var writer = PdfWriter.GetInstance(document, ms);
        document.Open();

        var bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        var titleFont = new Font(bf, 16, Font.BOLD);
        var headerFont = new Font(bf, 9, Font.BOLD);
        var cellFont = new Font(bf, 8, Font.NORMAL);

        document.Add(new Paragraph(title, titleFont));
        document.Add(new Paragraph(" "));

        var table = new PdfPTable(11);
        table.WidthPercentage = 100;

        var headers = new[] { "S.No", "Miti", "Invoice No", "Supplier", "Item", "Category", "Qty", "Rate", "Taxable", "VAT", "Total" };
        foreach (var h in headers)
        {
            var cell = new PdfPCell(new Phrase(h, headerFont));
            cell.BackgroundColor = BaseColor.LightGray;
            table.AddCell(cell);
        }

        foreach (var item in data)
        {
            table.AddCell(new PdfPCell(new Phrase(item.Sno.ToString(CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.Miti, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.InvoiceNo, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.SupplierName, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.ItemName, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.CategoryName, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.TaxableAmount.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.VatAmount.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.TotalAmount.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
        }

        if (data.Count > 0)
        {
            var totalTaxable = data.Sum(x => x.TaxableAmount);
            var totalVat = data.Sum(x => x.VatAmount);
            var total = data.Sum(x => x.TotalAmount);

            table.AddCell(new PdfPCell(new Phrase("Total", headerFont)) { Colspan = 8 });
            table.AddCell(new PdfPCell(new Phrase(totalTaxable.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(totalVat.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(total.ToString("N2", CultureInfo.InvariantCulture), cellFont)));
        }

        document.Add(table);
        document.Close();
        return ms.ToArray();
    }

    public byte[] GenerateCsv(List<ReportRowViewModel> data)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.WriteField("S.No");
        csv.WriteField("Miti");
        csv.WriteField("Invoice No");
        csv.WriteField("Supplier Name");
        csv.WriteField("Item Name");
        csv.WriteField("Category");
        csv.WriteField("Quantity");
        csv.WriteField("Rate");
        csv.WriteField("Taxable Amount");
        csv.WriteField("VAT Amount");
        csv.WriteField("Total Amount");
        csv.NextRecord();

        foreach (var item in data)
        {
            csv.WriteField(item.Sno);
            csv.WriteField(item.Miti);
            csv.WriteField(item.InvoiceNo);
            csv.WriteField(item.SupplierName);
            csv.WriteField(item.ItemName);
            csv.WriteField(item.CategoryName);
            csv.WriteField(item.Quantity);
            csv.WriteField(item.Rate);
            csv.WriteField(item.TaxableAmount);
            csv.WriteField(item.VatAmount);
            csv.WriteField(item.TotalAmount);
            csv.NextRecord();
        }

        writer.Flush();
        return ms.ToArray();
    }
}
