using System.Security.Cryptography;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IGoogleTokenValidator googleTokenValidator,
    IEmailSender emailSender,
    IRecaptchaValidator recaptchaValidator) : IAuthService
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(15);

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IGoogleTokenValidator _googleTokenValidator = googleTokenValidator;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IRecaptchaValidator _recaptchaValidator = recaptchaValidator;

    public async Task<RegistrationPendingDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _recaptchaValidator.ValidateAsync(dto.RecaptchaToken, cancellationToken))
        {
            throw new RecaptchaValidationException();
        }

        var repository = _unitOfWork.Repository<ApplicationUser>();
        var existing = await repository.FindAsync(u => u.Email == dto.Email, cancellationToken);
        if (existing.Count > 0)
        {
            throw new InvalidOperationException("Bu e-posta adresi zaten kayıtlı.");
        }

        var code = GenerateVerificationCode();
        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = UserRole.Yarismaci,
            IsEmailVerified = false,
            EmailVerificationCode = code,
            EmailVerificationCodeExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime)
        };

        await repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendVerificationEmailAsync(user, code, cancellationToken);

        return new RegistrationPendingDto(user.Email, "Kayıt alındı. E-postanıza gönderilen 6 haneli kodu girerek hesabınızı doğrulayın.");
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _recaptchaValidator.ValidateAsync(dto.RecaptchaToken, cancellationToken))
        {
            throw new RecaptchaValidationException();
        }

        var users = await _unitOfWork.Repository<ApplicationUser>().FindAsync(u => u.Email == dto.Email, cancellationToken);
        var user = users.FirstOrDefault();
        if (user is null || !user.IsActive || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        if (!user.IsEmailVerified)
        {
            throw new InvalidOperationException("E-posta adresiniz henüz doğrulanmadı. Lütfen e-postanıza gönderilen kodu girin.");
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto?> VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<ApplicationUser>();
        var users = await repository.FindAsync(u => u.Email == dto.Email, cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null
            || user.EmailVerificationCode is null
            || user.EmailVerificationCode != dto.Code
            || user.EmailVerificationCodeExpiresAt is null
            || user.EmailVerificationCodeExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        user.IsEmailVerified = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        repository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user);
    }

    public async Task<bool> ResendVerificationAsync(ResendVerificationDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<ApplicationUser>();
        var users = await repository.FindAsync(u => u.Email == dto.Email, cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null || user.IsEmailVerified)
        {
            return false;
        }

        var code = GenerateVerificationCode();
        user.EmailVerificationCode = code;
        user.EmailVerificationCodeExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime);
        user.UpdatedAt = DateTime.UtcNow;

        repository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendVerificationEmailAsync(user, code, cancellationToken);
        return true;
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await _googleTokenValidator.ValidateAsync(dto.IdToken, cancellationToken);
        if (!profile.EmailVerified)
        {
            throw new InvalidOperationException("Google hesabınızın e-posta adresi doğrulanmamış.");
        }

        var repository = _unitOfWork.Repository<ApplicationUser>();
        var users = await repository.FindAsync(u => u.Email == profile.Email, cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            // No local password will ever be used to sign in to this account, but the column is
            // NOT NULL — a random hash keeps it unguessable rather than leaving it blank. Google
            // already proved ownership of the email, so no verification code is needed either.
            user = new ApplicationUser
            {
                FullName = profile.FullName,
                Email = profile.Email,
                PasswordHash = _passwordHasher.Hash(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")),
                Role = UserRole.Yarismaci,
                IsEmailVerified = true
            };
            await repository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (!user.IsActive)
        {
            throw new InvalidOperationException("Bu hesap devre dışı bırakılmış.");
        }

        return BuildAuthResponse(user);
    }

    public async Task<ForgotPasswordResponseDto?> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<ApplicationUser>();
        var users = await repository.FindAsync(u => u.Email == dto.Email, cancellationToken);
        var user = users.FirstOrDefault();
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.Add(ResetTokenLifetime);

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiresAt = expiresAt;
        user.UpdatedAt = DateTime.UtcNow;

        repository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var html = $"""
            <p>Merhaba {user.FullName},</p>
            <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın (1 saat geçerlidir):</p>
            <p style="font-size:24px;font-weight:bold;letter-spacing:2px;">{token}</p>
            """;
        await _emailSender.SendAsync(user.Email, user.FullName, "Şifre Sıfırlama Kodu", html, cancellationToken);

        return new ForgotPasswordResponseDto("Şifre sıfırlama kodu e-postanıza gönderildi.");
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<ApplicationUser>();
        var users = await repository.FindAsync(u => u.Email == dto.Email, cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null
            || user.PasswordResetToken is null
            || user.PasswordResetToken != dto.ResetToken
            || user.PasswordResetTokenExpiresAt is null
            || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        repository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SendVerificationEmailAsync(ApplicationUser user, string code, CancellationToken cancellationToken)
    {
        var html = $"""
            <p>Merhaba {user.FullName},</p>
            <p>TEKNOFEST Yarışmacı Asistanı hesabınızı doğrulamak için aşağıdaki kodu girin (15 dakika geçerlidir):</p>
            <p style="font-size:28px;font-weight:bold;letter-spacing:4px;">{code}</p>
            """;
        await _emailSender.SendAsync(user.Email, user.FullName, "Hesap Doğrulama Kodu", html, cancellationToken);
    }

    private static string GenerateVerificationCode() => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

    private AuthResponseDto BuildAuthResponse(ApplicationUser user)
    {
        var (token, expiresAt) = _tokenService.GenerateToken(user);
        return new AuthResponseDto(user.Id, user.FullName, user.Email, user.Role, token, expiresAt);
    }
}
