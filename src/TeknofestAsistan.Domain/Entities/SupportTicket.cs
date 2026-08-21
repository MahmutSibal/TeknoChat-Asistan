using TeknofestAsistan.Domain.Common;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Domain.Entities;

public class SupportTicket : BaseEntity
{
    public int ChatQueryId { get; set; }
    public ChatQuery ChatQuery { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }

    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Acik;
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
