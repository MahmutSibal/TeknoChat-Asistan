using TeknofestAsistan.Domain.Common;

namespace TeknofestAsistan.Domain.Entities;

public class FaqEntry : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? SourceChatQueryId { get; set; }
    public ChatQuery? SourceChatQuery { get; set; }

    public int CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
