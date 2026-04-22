using GLMS.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
                // Set QuestPDF license (Community = free)
                QuestPDF.Settings.License = LicenseType.Community;

                var folderPath = Path.Combine(
                    _env.WebRootPath, "contracts", "generated");
                Directory.CreateDirectory(folderPath);

                var fileName = $"Contract_{contract.Id}_{Guid.NewGuid()}.pdf";
                var fullPath = Path.Combine(folderPath, fileName);

                var clientName = contract.Client?.Name ?? "N/A";
                var clientRegion = contract.Client?.Region ?? "N/A";
                var clientContact = contract.Client?.ContactDetails ?? "N/A";

                await Task.Run(() =>
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1.6f, Unit.Centimetre);
                            page.DefaultTextStyle(x => x.FontSize(9));

                            // ── HEADER ──────────────────────────
                            page.Header().Column(col =>
                            {
                                col.Item().Background("#1a1a2e")
                                   .Padding(12)
                                   .Row(row =>
                                   {
                                       row.RelativeItem().Text("TECHMOVE LOGISTICS")
                                           .FontSize(18).Bold()
                                           .FontColor("#ffffff");

                                       row.RelativeItem().AlignRight()
                                           .Text("SERVICE CONTRACT AGREEMENT")
                                           .FontSize(10).Bold()
                                           .FontColor("#6cff99");
                                   });

                                col.Item().Background("#f0fdf4")
                                   .Border(0.5f).BorderColor("#bbf7d0")
                                   .Padding(6)
                                   .Text(
                                       "ACTION REQUIRED: Download this contract, " +
                                       "sign it, and upload the signed copy via " +
                                       "the GLMS portal to activate your agreement.")
                                   .FontSize(8)
                                   .FontColor("#92400e");
                            });

                            page.Content().PaddingTop(10).Column(col =>
                            {
                                // ── CONTRACT DETAILS ────────────
                                col.Item().Text("CONTRACT DETAILS")
                                   .FontSize(9).Bold().FontColor("#1a1a2e");
                                col.Item().LineHorizontal(0.8f)
                                   .LineColor("#1a1a2e");
                                col.Item().PaddingTop(5)
                                   .Background("#f8f9fa")
                                   .Border(0.5f).BorderColor("#dee2e6")
                                   .Padding(7)
                                   .Table(tbl =>
                                   {
                                       tbl.ColumnsDefinition(c =>
                                       {
                                           c.RelativeColumn(1.3f);
                                           c.RelativeColumn(2f);
                                           c.RelativeColumn(1.3f);
                                           c.RelativeColumn(2f);
                                           c.RelativeColumn(1.5f);
                                           c.RelativeColumn(2f);
                                       });

                                       void LabelCell(string text) =>
                                           tbl.Cell().PaddingVertical(3)
                                              .Text(text).Bold()
                                              .FontSize(8).FontColor("#6c757d");

                                       void ValCell(string text) =>
                                           tbl.Cell().PaddingVertical(3)
                                              .Text(text).FontSize(8.5f)
                                              .FontColor("#1a1a2e");

                                       LabelCell("Contract No:");
                                       ValCell($"GLMS-{contract.Id:D5}");
                                       LabelCell("Date:");
                                       ValCell(DateTime.Now.ToString("dd MMM yyyy"));
                                       LabelCell("Service Level:");
                                       ValCell(contract.ServiceLevel);

                                       LabelCell("Client:");
                                       ValCell(clientName);
                                       LabelCell("Region:");
                                       ValCell(clientRegion);
                                       LabelCell("Contact:");
                                       ValCell(clientContact);

                                       LabelCell("Start Date:");
                                       ValCell(contract.StartDate.ToString("dd MMM yyyy"));
                                       LabelCell("End Date:");
                                       ValCell(contract.EndDate.ToString("dd MMM yyyy"));
                                       LabelCell("Status:");
                                       ValCell(contract.Status.ToString());
                                   });

                                col.Item().PaddingTop(8);

                                // ── PARTIES ─────────────────────
                                col.Item().Text("PARTIES TO THE AGREEMENT")
                                   .FontSize(9).Bold().FontColor("#1a1a2e");
                                col.Item().LineHorizontal(0.8f)
                                   .LineColor("#1a1a2e");
                                col.Item().PaddingTop(5)
                                   .Row(row =>
                                   {
                                       row.RelativeItem().Column(c =>
                                       {
                                           c.Item().Background("#1a1a2e")
                                              .Padding(6)
                                              .Text("THE COMPANY").Bold()
                                              .FontSize(8).FontColor("#ffffff");

                                           c.Item().Background("#f8f9fa")
                                              .Border(0.5f).BorderColor("#dee2e6")
                                              .Padding(8)
                                              .Text(t =>
                                              {
                                                  t.Line("TechMove Logistics (Pty) Ltd")
                                                   .Bold().FontSize(8.5f);
                                                  t.Line("Email: contracts@techmovelogistics.com")
                                                   .FontSize(8);
                                                  t.Line("Phone: [PHONE]  |  Reg: [REG NO]")
                                                   .FontSize(8).FontColor("#6c757d");
                                              });
                                       });

                                       row.ConstantItem(8);

                                       row.RelativeItem().Column(c =>
                                       {
                                           c.Item().Background("#1a1a2e")
                                              .Padding(6)
                                              .Text("THE CLIENT").Bold()
                                              .FontSize(8).FontColor("#ffffff");

                                           c.Item().Background("#f8f9fa")
                                              .Border(0.5f).BorderColor("#dee2e6")
                                              .Padding(8)
                                              .Text(t =>
                                              {
                                                  t.Line($"Company: {clientName}")
                                                   .Bold().FontSize(8.5f);
                                                  t.Line($"Contact: {clientContact}")
                                                   .FontSize(8);
                                                  t.Line($"Region: {clientRegion}")
                                                   .FontSize(8).FontColor("#6c757d");
                                              });
                                       });
                                   });

                                col.Item().PaddingTop(8);

                                // ── TERMS ───────────────────────
                                col.Item().Text("TERMS AND CONDITIONS")
                                   .FontSize(9).Bold().FontColor("#1a1a2e");
                                col.Item().LineHorizontal(0.8f)
                                   .LineColor("#1a1a2e");
                                col.Item().PaddingTop(5)
                                   .Row(row =>
                                   {
                                       var leftTerms = new[]
                                       {
                                           ("1. Services",
                                            "TechMove Logistics provides freight coordination " +
                                            "under the selected service level."),
                                           ("2. Payment",
                                            "Invoices due within 30 days. " +
                                            "Late payments attract prime rate + 2% interest."),
                                           ("3. Client Obligations",
                                            "Client must provide accurate shipment info and " +
                                            "comply with shipping regulations."),
                                           ("4. Liability",
                                            "Liability limited to value of affected shipment. " +
                                            "No indirect damages."),
                                           ("5. Termination",
                                            "Either party may terminate with 30 days notice " +
                                            "or immediately on breach."),
                                       };

                                       var rightTerms = new[]
                                       {
                                           ("6. Confidentiality",
                                            "Both parties keep all shared business " +
                                            "information confidential."),
                                           ("7. Force Majeure",
                                            "No liability for failure due to events beyond " +
                                            "reasonable control."),
                                           ("8. Governing Law",
                                            "Governed by the laws of the " +
                                            "Republic of South Africa."),
                                           ("9. Entire Agreement",
                                            "This document supersedes all prior " +
                                            "negotiations on this subject."),
                                           ("10. GLMS Portal",
                                            "Service requests may only be raised via GLMS " +
                                            "while this contract is Active."),
                                       };

                                       void TermsColumn(
                                           IContainer c,
                                           (string, string)[] items)
                                       {
                                           c.Background("#f8f9fa")
                                            .Border(0.5f).BorderColor("#dee2e6")
                                            .Padding(8)
                                            .Column(col =>
                                            {
                                                foreach (var (title, text) in items)
                                                {
                                                    col.Item().Text(t =>
                                                    {
                                                        t.Span($"{title}: ")
                                                         .Bold().FontSize(8);
                                                        t.Span(text)
                                                         .FontSize(8)
                                                         .FontColor("#374151");
                                                    });
                                                    col.Item().PaddingTop(3);
                                                }
                                            });
                                       }

                                       row.RelativeItem()
                                           .Element(c => TermsColumn(c, leftTerms));
                                       row.ConstantItem(8);
                                       row.RelativeItem()
                                           .Element(c => TermsColumn(c, rightTerms));
                                   });

                                col.Item().PaddingTop(8);

                                // ── SIGNATURES ──────────────────
                                col.Item().Text("AUTHORISATION AND SIGNATURES")
                                   .FontSize(9).Bold().FontColor("#1a1a2e");
                                col.Item().LineHorizontal(0.8f)
                                   .LineColor("#1a1a2e");
                                col.Item().PaddingTop(5)
                                   .Row(row =>
                                   {
                                       void SigColumn(IContainer c, string party)
                                       {
                                           c.Column(col =>
                                           {
                                               col.Item().Background("#1a1a2e")
                                                  .Padding(6).AlignCenter()
                                                  .Text($"FOR AND ON BEHALF OF {party}")
                                                  .Bold().FontSize(8)
                                                  .FontColor("#ffffff");

                                               col.Item().Background("#f8f9fa")
                                                  .Border(0.5f).BorderColor("#dee2e6")
                                                  .Padding(10)
                                                  .Column(inner =>
                                                  {
                                                      inner.Item().PaddingBottom(10)
                                                           .Text("").FontSize(8);
                                                      inner.Item()
                                                           .Text("Signature: _____________________________")
                                                           .FontSize(8.5f);
                                                      inner.Item().PaddingTop(6)
                                                           .Text("Full Name: ______________________________")
                                                           .FontSize(8.5f);
                                                      inner.Item().PaddingTop(6)
                                                           .Text("Designation: ___________________________")
                                                           .FontSize(8.5f);
                                                      inner.Item().PaddingTop(6)
                                                           .Text("Date: __________________________________")
                                                           .FontSize(8.5f);
                                                  });
                                           });
                                       }

                                       row.RelativeItem().Element(c =>
                                           SigColumn(c, "TECHMOVE LOGISTICS"));
                                       row.ConstantItem(8);
                                       row.RelativeItem().Element(c =>
                                           SigColumn(c, "THE CLIENT"));
                                   });

                                // ── WITNESS ─────────────────────
                                col.Item().PaddingTop(6)
                                   .Background("#f8f9fa")
                                   .Border(0.5f).BorderColor("#dee2e6")
                                   .Padding(7)
                                   .Row(row =>
                                   {
                                       row.RelativeItem()
                                          .Text(t =>
                                          {
                                              t.Span("Witness 1: ").Bold().FontSize(8);
                                              t.Span("Signature: ________________  " +
                                                     "Name: _____________________  " +
                                                     "Date: ____________")
                                               .FontSize(8);
                                          });
                                   });

                                col.Item().PaddingTop(4)
                                   .Background("#f8f9fa")
                                   .Border(0.5f).BorderColor("#dee2e6")
                                   .Padding(7)
                                   .Row(row =>
                                   {
                                       row.RelativeItem()
                                          .Text(t =>
                                          {
                                              t.Span("Witness 2: ").Bold().FontSize(8);
                                              t.Span("Signature: ________________  " +
                                                     "Name: _____________________  " +
                                                     "Date: ____________")
                                               .FontSize(8);
                                          });
                                   });
                            });

                            // ── FOOTER ──────────────────────────
                            page.Footer().PaddingTop(6).Column(col =>
                            {
                                col.Item().LineHorizontal(0.4f)
                                   .LineColor("#dee2e6");
                                col.Item().PaddingTop(4).AlignCenter()
                                   .Text("TechMove Logistics (Pty) Ltd  |  " +
                                         "GLMS — Global Logistics Management System  |  " +
                                         "contracts@techmovelogistics.com  |  " +
                                         "Only binding once signed by both parties.")
                                   .FontSize(7).FontColor("#6c757d");
                            });
                        });
                    }).GeneratePdf(fullPath);
                });

                return Path.Combine("contracts", "generated", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to generate PDF for contract {Id}", contract.Id);
                throw;
            }
        }
    }
}