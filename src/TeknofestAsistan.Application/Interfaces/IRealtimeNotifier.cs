namespace TeknofestAsistan.Application.Interfaces;

/// <summary>Pushes real-time events over SignalR. Purely additive UX — every caller must keep
/// working correctly even if a push silently fails (no connected client, transient error, etc.).</summary>
public interface IRealtimeNotifier
{
    /// <summary>One incremental token of an in-progress AI answer, matched to the asking client via
    /// the correlationId it supplied on ask. isFinal marks the last chunk for that answer.</summary>
    Task SendAnswerChunkAsync(int userId, string correlationId, string chunk, bool isFinal, CancellationToken cancellationToken = default);

    /// <summary>A competitor's escalated question has been resolved by Destek Ekibi.</summary>
    Task NotifyTicketResolvedAsync(int competitorUserId, int chatQueryId, string resolution, CancellationToken cancellationToken = default);

    /// <summary>A new question was escalated to a human — broadcast to Destek Ekibi/Sistem Yöneticisi.</summary>
    Task NotifyNewTicketAsync(int ticketId, string questionText, int competitionId, CancellationToken cancellationToken = default);
}
