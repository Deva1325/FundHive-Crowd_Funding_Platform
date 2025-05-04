using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Crowd_Funding_Platform.Models;

public class CreatorsList_PDF
{
    private readonly List<CreatorApplication> _creators;

    public CreatorsList_PDF(List<CreatorApplication> creators)
    {
        _creators = creators;
    }

    public byte[] GeneratePdf()
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Creators Report").FontSize(20).Bold().AlignCenter();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1); // Username
                        columns.RelativeColumn(2); // Email
                        columns.RelativeColumn(2); // Phone
                        columns.RelativeColumn(1); // Status
                        columns.RelativeColumn(2); // SubmissionDate
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Text("Username").Bold();
                        header.Cell().Text("Email").Bold();
                        header.Cell().Text("Phone").Bold();
                        header.Cell().Text("Status").Bold();
                        header.Cell().Text("Submitted On").Bold();
                    });

                    foreach (var creator in _creators)
                    {
                        table.Cell().Text(creator.User?.Username ?? "-");
                        table.Cell().Text(creator.User?.Email ?? "-");
                        table.Cell().Text(creator.User?.PhoneNumber ?? "-");
                        table.Cell().Text(creator.Status);
                        table.Cell().Text($"{creator.SubmissionDate:dd MMM yyyy}");

                    }
                });

                page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:dd MMM yyyy HH:mm}");
            });
        }).GeneratePdf();
    }
}
