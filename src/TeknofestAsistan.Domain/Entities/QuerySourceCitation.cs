using TeknofestAsistan.Domain.Common;

namespace TeknofestAsistan.Domain.Entities;

public class QuerySourceCitation : BaseEntity
{
    public int ChatQueryId { get; set; }
    public ChatQuery ChatQuery { get; set; } = null!;

    public int SourceDocumentId { get; set; }
    public SourceDocument SourceDocument { get; set; } = null!;

    public double RelevanceScore { get; set; }
}
