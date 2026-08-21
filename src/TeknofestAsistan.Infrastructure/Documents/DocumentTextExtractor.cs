using System.Text;
using DocumentFormat.OpenXml.Packaging;
using TeknofestAsistan.Application.Interfaces;
using UglyToad.PdfPig;

namespace TeknofestAsistan.Infrastructure.Documents;

public class DocumentTextExtractor : IDocumentTextExtractor
{
    private static readonly string[] SupportedExtensions = [".pdf", ".docx", ".txt"];

    public bool CanExtract(string fileName) =>
        SupportedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => ExtractFromPdf(fileStream),
            ".docx" => ExtractFromDocx(fileStream),
            ".txt" => await ExtractFromTextAsync(fileStream, cancellationToken),
            _ => throw new NotSupportedException(
                $"'{extension}' uzantılı dosyalardan metin çıkarma desteklenmiyor. Desteklenen türler: {string.Join(", ", SupportedExtensions)}")
        };
    }

    private static string ExtractFromPdf(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string ExtractFromDocx(Stream stream)
    {
        using var document = WordprocessingDocument.Open(stream, false);
        return document.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
    }

    private static async Task<string> ExtractFromTextAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
