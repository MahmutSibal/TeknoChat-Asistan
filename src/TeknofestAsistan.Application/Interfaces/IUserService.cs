using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface IUserService
{
    Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
}
