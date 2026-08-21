using TeknofestAsistan.Domain.Common;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Domain.Entities;

public class SourceDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public SourceDocumentType DocumentType { get; set; }

    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? Content { get; set; }

    public int UploadedByUserId { get; set; }
    public ApplicationUser UploadedByUser { get; set; } = null!;

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    public ICollection<QuerySourceCitation> Citations { get; set; } = new List<QuerySourceCitation>();
}
