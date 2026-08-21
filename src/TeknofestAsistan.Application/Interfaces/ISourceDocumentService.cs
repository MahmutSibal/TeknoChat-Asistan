using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface ISourceDocumentService
{
    Task<PagedResultDto<SourceDocumentDto>> GetAllAsync(int competitionId, int? categoryId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SourceDocumentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SourceDocumentDto> CreateAsync(CreateSourceDocumentDto dto, CancellationToken cancellationToken = default);

    /// <summary>Extracts text from an uploaded PDF/DOCX/TXT file and stores it the same way as CreateAsync
    /// (chunked + embedded). Throws NotSupportedException for unsupported file types.</summary>
    Task<SourceDocumentDto> CreateFromFileAsync(UploadSourceDocumentDto dto, Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Marks a source document inactive (pasife alma) so it no longer participates in retrieval, e.g. superseded by a newer şartname/kılavuz version.</summary>
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Re-generates embeddings for chunks that don't have one yet — e.g. chunks created
    /// while the AI embedding service was unreachable/misconfigured, or before the embedding model
    /// was changed. Optionally scoped to one competition. Returns how many chunks were fixed.</summary>
    Task<int> ReembedMissingChunksAsync(int? competitionId, CancellationToken cancellationToken = default);
}
