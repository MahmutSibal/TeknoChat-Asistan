using TeknofestAsistan.Domain.Common;

namespace TeknofestAsistan.Domain.Entities;

public class Competition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<SourceDocument> SourceDocuments { get; set; } = new List<SourceDocument>();
    public ICollection<ChatQuery> ChatQueries { get; set; } = new List<ChatQuery>();
    public ICollection<FaqEntry> FaqEntries { get; set; } = new List<FaqEntry>();
}
