namespace TeknofestAsistan.Application.Interfaces;

/// <summary>Turns an uploaded şartname/kılavuz file (PDF/DOCX/TXT) into plain text for chunking.</summary>
public interface IDocumentTextExtractor
{
    bool CanExtract(string fileName);

    /// <exception cref="NotSupportedException">The file extension isn't supported.</exception>
    Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
