using Microsoft.AspNetCore.SignalR;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Hubs;

public class SignalRNotifier(IHubContext<NotificationHub> hubContext) : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    public Task SendAnswerChunkAsync(int userId, string correlationId, string chunk, bool isFinal, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(NotificationHub.UserGroup(userId))
            .SendAsync("AnswerChunk", new { correlationId, chunk, isFinal }, cancellationToken);

    public Task NotifyTicketResolvedAsync(int competitorUserId, int chatQueryId, string resolution, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(NotificationHub.UserGroup(competitorUserId))
            .SendAsync("TicketResolved", new { chatQueryId, resolution }, cancellationToken);

    public Task NotifyNewTicketAsync(int ticketId, string questionText, int competitionId, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(NotificationHub.StaffGroup)
            .SendAsync("NewTicketCreated", new { ticketId, questionText, competitionId }, cancellationToken);
}
