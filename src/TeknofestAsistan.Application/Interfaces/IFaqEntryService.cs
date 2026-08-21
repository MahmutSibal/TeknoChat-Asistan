using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface IFaqEntryService
{
    Task<PagedResultDto<FaqEntryDto>> GetAllAsync(int competitionId, int? categoryId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<FaqEntryDto> CreateAsync(CreateFaqEntryDto dto, CancellationToken cancellationToken = default);
}
