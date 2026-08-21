using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface ISupportTicketService
{
    Task<PagedResultDto<SupportTicketDto>> GetOpenAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SupportTicketDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SupportTicketDto?> AssignAsync(int id, AssignSupportTicketDto dto, CancellationToken cancellationToken = default);
    Task<SupportTicketDto?> ResolveAsync(int id, ResolveSupportTicketDto dto, CancellationToken cancellationToken = default);
}
