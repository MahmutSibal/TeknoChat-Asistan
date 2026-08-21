using TeknofestAsistan.Domain.Common;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Domain.Entities;

public class ChatQuery : BaseEntity
{
    public int? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public ConfidenceLevel ConfidenceLevel { get; set; } = ConfidenceLevel.Yetersiz;
    public bool IsEscalated { get; set; }

    /// <summary>Only set when AnswerText is set — which RAG path produced it (full AI generation
    /// via Ollama, or the dependency-free keyword fallback used when Ollama is unreachable).</summary>
    public AnswerMode? AnswerMode { get; set; }

    /// <summary>Only set when IsEscalated — distinguishes "AI service unreachable" (temporary
    /// infrastructure issue) from "insufficient evidence in the source pool" (normal escalation),
    /// so the competitor sees an honest, specific reason instead of a generic "no answer".</summary>
    public string? EscalationReason { get; set; }

    public ICollection<QuerySourceCitation> Citations { get; set; } = new List<QuerySourceCitation>();
    public SupportTicket? SupportTicket { get; set; }
}
