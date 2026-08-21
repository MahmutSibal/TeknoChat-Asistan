using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Application.Services;

public class FaqEntryService(IUnitOfWork unitOfWork) : IFaqEntryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResultDto<FaqEntryDto>> GetAllAsync(
        int competitionId, int? categoryId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, totalCount) = await _unitOfWork.Repository<FaqEntry>().FindPagedAsync(
            f => f.CompetitionId == competitionId && f.IsActive && (categoryId == null || f.CategoryId == categoryId),
            pageNumber, pageSize, cancellationToken);
        return new PagedResultDto<FaqEntryDto>(items.Select(ToDto).ToList(), pageNumber, pageSize, totalCount);
    }

    public async Task<FaqEntryDto> CreateAsync(CreateFaqEntryDto dto, CancellationToken cancellationToken = default)
    {
        var entry = new FaqEntry
        {
            Question = dto.Question,
            Answer = dto.Answer,
            CompetitionId = dto.CompetitionId,
            CategoryId = dto.CategoryId,
            CreatedByUserId = dto.CreatedByUserId,
            SourceChatQueryId = dto.SourceChatQueryId,
            IsActive = true
        };
        await _unitOfWork.Repository<FaqEntry>().AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entry);
    }

    private static FaqEntryDto ToDto(FaqEntry f) => new(f.Id, f.Question, f.Answer, f.CompetitionId, f.CategoryId, f.IsActive, f.CreatedAt);
}
