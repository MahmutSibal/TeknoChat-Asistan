using System.ComponentModel.DataAnnotations;

namespace TeknofestAsistan.Application.Dtos;

public record CompetitionDto(int Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);

public record CreateCompetitionDto(
    [Required, MaxLength(200)] string Name,
    string? Description);

public record UpdateCompetitionDto(
    [Required, MaxLength(200)] string Name,
    string? Description,
    bool IsActive);
