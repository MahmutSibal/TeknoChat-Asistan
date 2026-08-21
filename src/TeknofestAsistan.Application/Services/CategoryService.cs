using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Application.Services;

public class CategoryService(IUnitOfWork unitOfWork) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<CategoryDto>> GetByCompetitionAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<Category>()
            .FindAsync(c => c.CompetitionId == competitionId, cancellationToken);
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, cancellationToken);
        return category is null ? null : ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            CompetitionId = dto.CompetitionId,
            Name = dto.Name,
            Description = dto.Description
        };
        await _unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Category>();
        var category = await repository.GetByIdAsync(id, cancellationToken);
        if (category is null) return null;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.UpdatedAt = DateTime.UtcNow;

        repository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    private static CategoryDto ToDto(Category c) => new(c.Id, c.CompetitionId, c.Name, c.Description);
}
