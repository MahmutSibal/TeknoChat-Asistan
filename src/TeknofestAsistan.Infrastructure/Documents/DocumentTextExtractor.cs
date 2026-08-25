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

        using var buffered = new MemoryStream();
        await fileStream.CopyToAsync(buffered, cancellationToken);
        buffered.Position = 0;

        if (!HasValidSignature(buffered, extension))
        {
            throw new InvalidOperationException(
                $"Dosya içeriği '{extension}' uzantısıyla uyuşmuyor. Dosya bozulmuş veya yeniden adlandırılmış farklı türde bir dosya olabilir.");
        }
        buffered.Position = 0;

        try
        {
            return extension switch
            {
                ".pdf" => ExtractFromPdf(buffered),
                ".docx" => ExtractFromDocx(buffered),
                ".txt" => await ExtractFromTextAsync(buffered, cancellationToken),
                _ => throw new NotSupportedException(
                    $"'{extension}' uzantılı dosyalardan metin çıkarma desteklenmiyor. Desteklenen türler: {string.Join(", ", SupportedExtensions)}")
            };
        }
        catch (Exception ex) when (ex is not NotSupportedException and not InvalidOperationException and not OperationCanceledException)
        {
            throw new InvalidOperationException("Dosya okunamadı; bozuk veya desteklenmeyen bir içerik olabilir.", ex);
        }
    }

    /// Uzantıya güvenmek yerine dosyanın gerçek baytlarını (magic number) doğrular — böylece
    /// örneğin zararlı bir çalıştırılabilir dosya .pdf uzantısıyla yeniden adlandırılıp
    /// PDF/DOCX ayrıştırıcısına doğrudan verilemez.
    private static bool HasValidSignature(MemoryStream stream, string extension)
    {
        var buffer = stream.ToArray();
        return extension switch
        {
            ".pdf" => buffer.Length >= 5 && buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46 && buffer[4] == 0x2D, // %PDF-
            ".docx" => buffer.Length >= 4 && buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04, // PK\x03\x04 (ZIP/OOXML)
            ".txt" => true, // düz metnin güvenilir bir imzası yok
            _ => false
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
