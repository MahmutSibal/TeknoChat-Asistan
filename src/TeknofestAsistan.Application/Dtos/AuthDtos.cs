using System.ComponentModel.DataAnnotations;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Dtos;

/// <summary>Public self-service registration. Always creates a Yarismaci (competitor) account —
/// internal roles are provisioned by a Sistem Yöneticisi via UsersController instead.</summary>
public record RegisterDto(
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [Required] string RecaptchaToken);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required] string RecaptchaToken);

public record AuthResponseDto(int UserId, string FullName, string Email, UserRole Role, string Token, DateTime ExpiresAt);

/// <summary>Registration no longer logs the competitor in directly — the account stays unverified
/// until they submit the code emailed to them via /api/auth/verify-email.</summary>
public record RegistrationPendingDto(string Email, string Message);

public record VerifyEmailDto(
    [Required, EmailAddress] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Code);

public record ResendVerificationDto([Required, EmailAddress] string Email);

public record ForgotPasswordDto([Required, EmailAddress] string Email);

public record ForgotPasswordResponseDto(string Message);

public record ResetPasswordDto(
    [Required, EmailAddress] string Email,
    [Required] string ResetToken,
    [Required, MinLength(8), MaxLength(100)] string NewPassword);

/// <summary>Google Identity Services ID token from the frontend's "Sign in with Google" button.
/// Finds-or-creates the account (new accounts default to Yarismaci) after server-side verification.</summary>
public record GoogleLoginDto([Required] string IdToken);
