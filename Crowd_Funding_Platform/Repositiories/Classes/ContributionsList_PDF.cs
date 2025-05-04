using Crowd_Funding_Platform.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class ContributionsList_PDF : IDocument
    {
        private readonly List<Contribution> _contributions;

        public ContributionsList_PDF(List<Contribution> contributions)
        {
            _contributions = contributions;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FundHive").FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("Contributions Report").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(100).Height(50).Placeholder(); // Add Logo here if needed
                });

                // Table Content
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Name
                        columns.RelativeColumn(2); // Email
                        columns.RelativeColumn(1.5f); // Amount
                        columns.RelativeColumn(1.5f); // Payment Status
                        columns.RelativeColumn(2); // Last Contributed Date
                    });

                    // Header Row
                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Name");
                        header.Cell().Element(CellHeaderStyle).Text("Email");
                        header.Cell().Element(CellHeaderStyle).Text("Amount");
                        header.Cell().Element(CellHeaderStyle).Text("Status");
                        header.Cell().Element(CellHeaderStyle).Text("Date");
                    });

                    // Table Body
                    bool isEven = false;
                    foreach (var item in _contributions)
                    {
                        isEven = !isEven;
                        var bg = isEven ? Colors.Grey.Lighten5 : Colors.White;

                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(item.Contributor?.Username ?? "-");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(item.Contributor?.Email ?? "-");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text($"₹{item.TotalContribution:F2}");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(item.PaymentStatus ?? "-");
                        //table.Cell().Element(c => CellBodyStyle(c, bg)).Text(item.Date.ToString("dd MMM yyyy"));
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text($"{item.Date:dd MMM}");
                    }
                });

                // Footer
                page.Footer().AlignCenter().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Generated on ").FontSize(10);
                        t.Span(DateTime.Now.ToString("dd MMM yyyy")).SemiBold().FontSize(10);
                    });

                    row.ConstantItem(100).AlignRight().Text(t =>
                    {
                        t.CurrentPageNumber().FontSize(10);
                        t.Span(" / ");
                        t.TotalPages().FontSize(10);
                    });
                });
            });
        }

        private static IContainer CellHeaderStyle(IContainer container) => container
            .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
            .PaddingVertical(6).PaddingHorizontal(5)
            .Background(Colors.Blue.Medium)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Darken2);

        private static IContainer CellBodyStyle(IContainer container, string bgColor) => container
            .PaddingVertical(5).PaddingHorizontal(5)
            .Background(bgColor)
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2);
    }
}
