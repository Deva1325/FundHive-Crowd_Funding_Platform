using Crowd_Funding_Platform.Models;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class CampaignsList_PDF : IDocument
    {
        private readonly List<Campaign> _campaigns;

        public CampaignsList_PDF(List<Campaign> campaigns)
        {
            _campaigns = campaigns;
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
                        col.Item().Text("Campaign Report").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(100).Height(50).Placeholder(); // Logo
                });

                // Content Table
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Title
                        columns.RelativeColumn(2); // Category
                        columns.RelativeColumn(2); // Dates
                        columns.RelativeColumn(1.5f); // Status
                        columns.RelativeColumn(1.5f); // Contributors
                    });

                    // Table Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Title");
                        header.Cell().Element(CellHeaderStyle).Text("Category");
                        header.Cell().Element(CellHeaderStyle).Text("Start - End");
                        header.Cell().Element(CellHeaderStyle).Text("Status");
                        header.Cell().Element(CellHeaderStyle).Text("Contributors");
                    });

                    // Table Body
                    bool isEven = false;
                    foreach (var camp in _campaigns)
                    {
                        isEven = !isEven;
                        var bg = isEven ? Colors.Grey.Lighten5 : Colors.White;

                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(camp.Title ?? "-");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(camp.Category.Name ?? "-");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text($"{camp.StartDate:dd MMM} - {camp.EndDate:dd MMM}");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(camp.Status ?? "-");
                        table.Cell().Element(c => CellBodyStyle(c, bg)).Text(camp.TotalContributors.ToString());
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
