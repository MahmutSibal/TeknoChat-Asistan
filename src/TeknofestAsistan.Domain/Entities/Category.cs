using TeknofestAsistan.Domain.Common;

namespace TeknofestAsistan.Domain.Entities;

public class Category : BaseEntity
{
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<SourceDocument> SourceDocuments { get; set; } = new List<SourceDocument>();
    public ICollection<ChatQuery> ChatQueries { get; set; } = new List<ChatQuery>();
    public ICollection<FaqEntry> FaqEntries { get; set; } = new List<FaqEntry>();
}
