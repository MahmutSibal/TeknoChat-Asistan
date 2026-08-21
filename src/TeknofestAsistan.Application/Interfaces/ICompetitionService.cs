using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface ICompetitionService
{
    Task<PagedResultDto<CompetitionDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<CompetitionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CompetitionDto> CreateAsync(CreateCompetitionDto dto, CancellationToken cancellationToken = default);
    Task<CompetitionDto?> UpdateAsync(int id, UpdateCompetitionDto dto, CancellationToken cancellationToken = default);
}
