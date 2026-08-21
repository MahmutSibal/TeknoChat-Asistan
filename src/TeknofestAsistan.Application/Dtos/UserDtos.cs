using System.ComponentModel.DataAnnotations;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Dtos;

public record UserDto(int Id, string FullName, string Email, UserRole Role, bool IsActive);

public record CreateUserDto(
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    UserRole Role);
