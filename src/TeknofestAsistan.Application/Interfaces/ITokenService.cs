using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user);
}
