using System.ComponentModel.DataAnnotations;

namespace TeknofestAsistan.Application.Dtos;

public record CategoryDto(int Id, int CompetitionId, string Name, string? Description);

public record CreateCategoryDto(
    [Range(1, int.MaxValue)] int CompetitionId,
    [Required, MaxLength(200)] string Name,
    string? Description);

public record UpdateCategoryDto(
    [Required, MaxLength(200)] string Name,
    string? Description);
