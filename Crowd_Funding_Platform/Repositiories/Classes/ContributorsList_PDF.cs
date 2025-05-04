using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class ContributorsList_PDF : IDocument
    {
        private readonly List<Contribution> _contributors;

        public ContributorsList_PDF(List<Contribution> contributors)
        {
            _contributors = contributors;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FundHive").FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("Contributors Report").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(100).Height(50).Placeholder(); // Logo placeholder
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Username
                        columns.RelativeColumn(2); // Campaign
                        columns.RelativeColumn(2); // Category
                        columns.RelativeColumn(2); // Date
                        columns.RelativeColumn(1.5f); // Amount
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("Username");
                        header.Cell().Element(CellHeaderStyle).Text("Campaign");
                        header.Cell().Element(CellHeaderStyle).Text("Category");
                        header.Cell().Element(CellHeaderStyle).Text("Date");
                        header.Cell().Element(CellHeaderStyle).Text("Amount");
                    });

                    // Body
                    bool isEven = false;
                    foreach (var c in _contributors)
                    {
                        isEven = !isEven;
                        var bg = isEven ? Colors.Grey.Lighten5 : Colors.White;

                        table.Cell().Element(cell => CellBodyStyle(cell, bg)).Text(c.Contributor?.Username ?? "-");
                        table.Cell().Element(cell => CellBodyStyle(cell, bg)).Text(c.Campaign?.Title ?? "-");
                        table.Cell().Element(cell => CellBodyStyle(cell, bg)).Text(c.Campaign?.Category?.Name ?? "-");
                        table.Cell().Element(cell => CellBodyStyle(cell, bg)).Text($"{c.Date:dd MMM yyyy}");
                        table.Cell().Element(cell => CellBodyStyle(cell, bg)).Text($"₹{c.Amount:N2}");
                    }
                });

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
