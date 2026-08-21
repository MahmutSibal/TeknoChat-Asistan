using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Application.Services;

public class CompetitionService(IUnitOfWork unitOfWork) : ICompetitionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResultDto<CompetitionDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, totalCount) = await _unitOfWork.Repository<Competition>().FindPagedAsync(_ => true, pageNumber, pageSize, cancellationToken);
        return new PagedResultDto<CompetitionDto>(items.Select(ToDto).ToList(), pageNumber, pageSize, totalCount);
    }

    public async Task<CompetitionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var competition = await _unitOfWork.Repository<Competition>().GetByIdAsync(id, cancellationToken);
        return competition is null ? null : ToDto(competition);
    }

    public async Task<CompetitionDto> CreateAsync(CreateCompetitionDto dto, CancellationToken cancellationToken = default)
    {
        var competition = new Competition { Name = dto.Name, Description = dto.Description };
        await _unitOfWork.Repository<Competition>().AddAsync(competition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(competition);
    }

    public async Task<CompetitionDto?> UpdateAsync(int id, UpdateCompetitionDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Competition>();
        var competition = await repository.GetByIdAsync(id, cancellationToken);
        if (competition is null) return null;

        competition.Name = dto.Name;
        competition.Description = dto.Description;
        competition.IsActive = dto.IsActive;
        competition.UpdatedAt = DateTime.UtcNow;

        repository.Update(competition);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(competition);
    }

    private static CompetitionDto ToDto(Competition c) => new(c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt);
}
