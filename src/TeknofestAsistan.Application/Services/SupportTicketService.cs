using Microsoft.Extensions.Logging;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Services;

public class SupportTicketService(IUnitOfWork unitOfWork, IRealtimeNotifier realtimeNotifier, ILogger<SupportTicketService> logger) : ISupportTicketService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRealtimeNotifier _realtimeNotifier = realtimeNotifier;
    private readonly ILogger<SupportTicketService> _logger = logger;

    public async Task<PagedResultDto<SupportTicketDto>> GetOpenAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, totalCount) = await _unitOfWork.Repository<SupportTicket>().FindPagedAsync(
            t => t.Status == SupportTicketStatus.Acik || t.Status == SupportTicketStatus.Islemde,
            pageNumber, pageSize, cancellationToken);
        var dtos = await ToDtosAsync(items, cancellationToken);
        return new PagedResultDto<SupportTicketDto>(dtos, pageNumber, pageSize, totalCount);
    }

    public async Task<SupportTicketDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _unitOfWork.Repository<SupportTicket>().GetByIdAsync(id, cancellationToken);
        if (ticket is null) return null;
        var dtos = await ToDtosAsync([ticket], cancellationToken);
        return dtos[0];
    }

    public async Task<SupportTicketDto?> AssignAsync(int id, AssignSupportTicketDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<SupportTicket>();
        var ticket = await repository.GetByIdAsync(id, cancellationToken);
        if (ticket is null) return null;

        ticket.AssignedToUserId = dto.AssignedToUserId;
        ticket.Status = SupportTicketStatus.Islemde;
        ticket.UpdatedAt = DateTime.UtcNow;

        repository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var dtos = await ToDtosAsync([ticket], cancellationToken);
        return dtos[0];
    }

    public async Task<SupportTicketDto?> ResolveAsync(int id, ResolveSupportTicketDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<SupportTicket>();
        var ticket = await repository.GetByIdAsync(id, cancellationToken);
        if (ticket is null) return null;

        ticket.Resolution = dto.Resolution;
        ticket.Status = SupportTicketStatus.Cozuldu;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        repository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var chatQuery = await _unitOfWork.Repository<ChatQuery>().GetByIdAsync(ticket.ChatQueryId, cancellationToken);
        if (chatQuery?.UserId is { } competitorUserId)
        {
            try
            {
                await _realtimeNotifier.NotifyTicketResolvedAsync(competitorUserId, chatQuery.Id, dto.Resolution, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gerçek zamanlı bildirim gönderilemedi (yok sayılıyor).");
            }
        }

        var dtos = await ToDtosAsync([ticket], cancellationToken);
        return dtos[0];
    }

    private async Task<IReadOnlyList<SupportTicketDto>> ToDtosAsync(IReadOnlyList<SupportTicket> tickets, CancellationToken cancellationToken)
    {
        var queryIds = tickets.Select(t => t.ChatQueryId).Distinct().ToList();
        var queries = await _unitOfWork.Repository<ChatQuery>().FindAsync(q => queryIds.Contains(q.Id), cancellationToken);
        var questionById = queries.ToDictionary(q => q.Id, q => q.QuestionText);
        var competitionById = queries.ToDictionary(q => q.Id, q => q.CompetitionId);

        return tickets.Select(t => new SupportTicketDto(
            t.Id, t.ChatQueryId, questionById.GetValueOrDefault(t.ChatQueryId, string.Empty),
            competitionById.GetValueOrDefault(t.ChatQueryId), t.AssignedToUserId, t.Status, t.Resolution,
            t.CreatedAt, t.ResolvedAt)).ToList();
    }
}
