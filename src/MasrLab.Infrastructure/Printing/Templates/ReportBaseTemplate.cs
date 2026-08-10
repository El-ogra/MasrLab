using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MasrLab.Infrastructure.Printing.Templates;

public abstract class ReportBaseTemplate : IDocument
{
    public DocumentMetadata GetMetadata() => new()
    {
        Title = Title,
        Author = "MasrLab",
        Subject = "MasrLab laboratory report"
    };

    protected abstract string Title { get; }
    protected virtual PageSize PageSize => PageSizes.A4;
    protected abstract void ComposeContent(IContainer container);

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSize);
            page.Margin(35);
            page.DefaultTextStyle(style => style.FontSize(10));
            page.Header().Text(Title).SemiBold().FontSize(18);
            page.Content().PaddingVertical(15).Element(ComposeContent);
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("MasrLab • ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    public byte[] RenderPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return this.GeneratePdf();
    }
}
