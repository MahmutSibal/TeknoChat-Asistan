using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Application.Services;

public class UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : IUserService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, totalCount) = await _unitOfWork.Repository<ApplicationUser>().FindPagedAsync(_ => true, pageNumber, pageSize, cancellationToken);
        return new PagedResultDto<UserDto>(items.Select(ToDto).ToList(), pageNumber, pageSize, totalCount);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(id, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<ApplicationUser>();
        var existing = await repository.FindAsync(u => u.Email == dto.Email, cancellationToken);
        if (existing.Count > 0)
        {
            throw new InvalidOperationException("Bu e-posta adresi zaten kayıtlı.");
        }

        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = dto.Role
        };
        await repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(user);
    }

    private static UserDto ToDto(ApplicationUser u) => new(u.Id, u.FullName, u.Email, u.Role, u.IsActive);
}
