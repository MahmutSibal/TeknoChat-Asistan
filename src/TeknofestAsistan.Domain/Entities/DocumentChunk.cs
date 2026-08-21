using TeknofestAsistan.Domain.Common;

namespace TeknofestAsistan.Domain.Entities;

public class DocumentChunk : BaseEntity
{
    public int SourceDocumentId { get; set; }
    public SourceDocument SourceDocument { get; set; } = null!;

    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;

    /// <summary>JSON-serialized embedding vector; populated by the embedding service.</summary>
    public string? Embedding { get; set; }
}
