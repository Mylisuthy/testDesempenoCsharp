using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Infrastructure.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        // License setup for Community (Free)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(string htmlContent)
    {
        // Note: QuestPDF builds PDFs via a Fluent API, not HTML conversion.
        // For this task, we will create a simple layout. 
        // The "htmlContent" input is kept for signature compatibility but we will parse/use it as raw text or structured data if possible.
        // However, the prompt implies "Generate CV". 
        // For simplicity in this demo, we will create a PDF document that prints the received content.
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("TalentosPlus - Hoja de Vida")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Item().Text(htmlContent); // Treating input as plain text for now or simple rendering
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
