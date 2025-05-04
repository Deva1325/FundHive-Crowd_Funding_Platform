using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using Crowd_Funding_Platform.Models;

public class CategoriesList_PDF : IDocument
{
    private readonly List<Category> _categories;

    public CategoriesList_PDF(List<Category> categories)
    {
        _categories = categories;
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
                    col.Item().Text("Category Report").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(100).Height(50).Placeholder();
            });

            page.Content().PaddingVertical(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1); // Category ID
                    columns.RelativeColumn(2); // Name
                    columns.RelativeColumn(2); // Total Contributions
                    columns.RelativeColumn(3); // Description
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeaderStyle).Text("ID");
                    header.Cell().Element(CellHeaderStyle).Text("Name");
                    header.Cell().Element(CellHeaderStyle).Text("Total Contributions");
                    header.Cell().Element(CellHeaderStyle).Text("Description");
                });

                bool isEven = false;
                foreach (var category in _categories)
                {
                    isEven = !isEven;
                    var bg = isEven ? Colors.Grey.Lighten5 : Colors.White;

                    table.Cell().Element(c => CellBodyStyle(c, bg)).Text(category.CategoryId.ToString());
                    table.Cell().Element(c => CellBodyStyle(c, bg)).Text(category.Name ?? "-");
                    table.Cell().Element(c => CellBodyStyle(c, bg)).Text(category.TotalContributions.ToString());
                    table.Cell().Element(c => CellBodyStyle(c, bg)).Text(category.Description ?? "-");
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
