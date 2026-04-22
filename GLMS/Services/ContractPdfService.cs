using GLMS.Web.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace GLMS.Web.Services
{
    public class ContractPdfService : IContractPdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ContractPdfService> _logger;

        public ContractPdfService(
            IWebHostEnvironment env,
            ILogger<ContractPdfService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> GenerateContractPdfAsync(Contract contract)
        {
            try
            {
                var folderPath = Path.Combine(
                    _env.WebRootPath, "contracts", "generated");
                Directory.CreateDirectory(folderPath);

                var fileName = $"Contract_{contract.Id}_{Guid.NewGuid()}.pdf";
                var fullPath = Path.Combine(folderPath, fileName);

                await Task.Run(() =>
                {
                    using var writer = new PdfWriter(fullPath);
                    using var pdf = new PdfDocument(writer);
                    using var doc = new Document(pdf);

                    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    var darkColor = new DeviceRgb(26, 26, 46);
                    var grayColor = new DeviceRgb(100, 100, 100);
                    var greenColor = new DeviceRgb(25, 135, 84);

                    // Header
                    doc.Add(new Paragraph("TECHM OVE LOGISTICS")
                        .SetFont(boldFont).SetFontSize(22)
                        .SetFontColor(darkColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(4));

                    doc.Add(new Paragraph("GLOBAL LOGISTICS MANAGEMENT SYSTEM")
                        .SetFont(normalFont).SetFontSize(11)
                        .SetFontColor(grayColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(4));

                    doc.Add(new Paragraph("SERVICE CONTRACT AGREEMENT")
                        .SetFont(boldFont).SetFontSize(14)
                        .SetFontColor(greenColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20));

                    // Divider
                    doc.Add(new LineSeparator(
                        new iText.Kernel.Pdf.Canvas.Draw.SolidLine(1.5f))
                        .SetMarginBottom(20));

                    // Contract details section
                    doc.Add(new Paragraph("CONTRACT DETAILS")
                        .SetFont(boldFont).SetFontSize(12)
                        .SetFontColor(darkColor).SetMarginBottom(10));

                    AddDetailRow(doc, boldFont, normalFont,
                        "Contract Number:", $"GLMS-{contract.Id:D5}");
                    AddDetailRow(doc, boldFont, normalFont,
                        "Client Name:", contract.Client?.Name ?? "N/A");
                    AddDetailRow(doc, boldFont, normalFont,
                        "Client Region:", contract.Client?.Region ?? "N/A");
                    AddDetailRow(doc, boldFont, normalFont,
                        "Contact Details:", contract.Client?.ContactDetails ?? "N/A");
                    AddDetailRow(doc, boldFont, normalFont,
                        "Service Level:", contract.ServiceLevel);
                    AddDetailRow(doc, boldFont, normalFont,
                        "Contract Status:", contract.Status.ToString());
                    AddDetailRow(doc, boldFont, normalFont,
                        "Effective From:", contract.StartDate.ToString("dd MMMM yyyy"));
                    AddDetailRow(doc, boldFont, normalFont,
                        "Effective Until:", contract.EndDate.ToString("dd MMMM yyyy"));
                    AddDetailRow(doc, boldFont, normalFont,
                        "Date Generated:", DateTime.Now.ToString("dd MMMM yyyy"));

                    doc.Add(new Paragraph("\n"));

                    // Terms section
                    doc.Add(new LineSeparator(
                        new iText.Kernel.Pdf.Canvas.Draw.SolidLine(1f))
                        .SetMarginBottom(15));

                    doc.Add(new Paragraph("TERMS AND CONDITIONS")
                        .SetFont(boldFont).SetFontSize(12)
                        .SetFontColor(darkColor).SetMarginBottom(10));

                    var terms = new[]
                    {
                        "1. This agreement is entered into between TechMove Logistics " +
                           "(the \"Company\") and the Client named above.",
                        "2. The Company agrees to provide logistics and freight coordination " +
                           $"services under the {contract.ServiceLevel} service level.",
                        "3. This contract is valid from " +
                           $"{contract.StartDate:dd MMMM yyyy} to " +
                           $"{contract.EndDate:dd MMMM yyyy}.",
                        "4. The Client agrees to comply with all applicable international " +
                           "shipping regulations and provide accurate shipment details.",
                        "5. Service requests may only be raised while this contract " +
                           "is in Active status.",
                        "6. Either party may terminate this agreement with 30 days " +
                           "written notice.",
                        "7. All disputes shall be governed by and construed in " +
                           "accordance with South African law.",
                        "8. The Company reserves the right to place contracts On Hold " +
                           "pending review of compliance or outstanding obligations.",
                        "9. This document must be signed by an authorised representative " +
                           "of the Client and returned to TechMove Logistics.",
                        "10. A signed copy uploaded via the GLMS portal constitutes " +
                            "acceptance of all terms herein."
                    };

                    foreach (var term in terms)
                    {
                        doc.Add(new Paragraph(term)
                            .SetFont(normalFont).SetFontSize(10)
                            .SetFontColor(grayColor).SetMarginBottom(6));
                    }

                    doc.Add(new Paragraph("\n\n"));

                    // Signature section
                    doc.Add(new LineSeparator(
                        new iText.Kernel.Pdf.Canvas.Draw.SolidLine(1f))
                        .SetMarginBottom(20));

                    doc.Add(new Paragraph("AUTHORISATION & SIGNATURES")
                        .SetFont(boldFont).SetFontSize(12)
                        .SetFontColor(darkColor).SetMarginBottom(20));

                    var sigTable = new Table(2).UseAllAvailableWidth();
                    sigTable.AddCell(CreateSignatureCell(
                        "For and on behalf of TechMove Logistics:",
                        boldFont, normalFont));
                    sigTable.AddCell(CreateSignatureCell(
                        $"For and on behalf of {contract.Client?.Name ?? "Client"}:",
                        boldFont, normalFont));
                    doc.Add(sigTable);

                    // Footer
                    doc.Add(new Paragraph(
                        $"\nContract Reference: GLMS-{contract.Id:D5} | " +
                        $"Generated: {DateTime.Now:dd MMM yyyy HH:mm} | " +
                        $"TechMove Logistics GLMS")
                        .SetFont(normalFont).SetFontSize(8)
                        .SetFontColor(grayColor)
                        .SetTextAlignment(TextAlignment.CENTER));
                });

                return Path.Combine("contracts", "generated", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate PDF for contract {Id}",
                    contract.Id);
                throw;
            }
        }

        private void AddDetailRow(Document doc, PdfFont bold, PdfFont normal,
            string label, string value)
        {
            var table = new Table(new float[] { 2, 5 })
                .UseAllAvailableWidth().SetMarginBottom(5);

            table.AddCell(new Cell()
                .Add(new Paragraph(label).SetFont(bold).SetFontSize(10))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingBottom(2));

            table.AddCell(new Cell()
                .Add(new Paragraph(value).SetFont(normal).SetFontSize(10))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingBottom(2));

            doc.Add(table);
        }

        private Cell CreateSignatureCell(string label,
            PdfFont bold, PdfFont normal)
        {
            var cell = new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPaddingRight(15);

            cell.Add(new Paragraph(label)
                .SetFont(bold).SetFontSize(10).SetMarginBottom(30));
            cell.Add(new Paragraph("Signature: _______________________")
                .SetFont(normal).SetFontSize(10).SetMarginBottom(10));
            cell.Add(new Paragraph("Full Name: _______________________")
                .SetFont(normal).SetFontSize(10).SetMarginBottom(10));
            cell.Add(new Paragraph("Designation: ____________________")
                .SetFont(normal).SetFontSize(10).SetMarginBottom(10));
            cell.Add(new Paragraph("Date: ___________________________")
                .SetFont(normal).SetFontSize(10));

            return cell;
        }
    }
}