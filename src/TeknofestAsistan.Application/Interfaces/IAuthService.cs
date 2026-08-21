using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface IAuthService
{
    /// <exception cref="InvalidOperationException">Email already registered.</exception>
    Task<RegistrationPendingDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

    /// <exception cref="InvalidOperationException">Credentials are correct but the email isn't verified yet.</exception>
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task<AuthResponseDto?> VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken = default);
    Task<bool> ResendVerificationAsync(ResendVerificationDto dto, CancellationToken cancellationToken = default);

    /// <summary>Finds the account by the Google-verified email, or creates one (as Yarismaci) if
    /// it's the first time this email has signed in.</summary>
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto, CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponseDto?> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
}
