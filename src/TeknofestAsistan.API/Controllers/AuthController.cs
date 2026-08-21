using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationPendingDto>> Register(RegisterDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto, cancellationToken);
            return Ok(result);
        }
        catch (RecaptchaValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<AuthResponseDto>> VerifyEmail(VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(dto, cancellationToken);
        return result is null ? BadRequest(new { message = "Kod geçersiz veya süresi dolmuş." }) : Ok(result);
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationDto dto, CancellationToken cancellationToken)
    {
        var sent = await _authService.ResendVerificationAsync(dto, cancellationToken);
        return sent ? NoContent() : NotFound(new { message = "Doğrulama bekleyen bir hesap bulunamadı." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(dto, cancellationToken);
            return result is null ? Unauthorized(new { message = "E-posta veya şifre hatalı." }) : Ok(result);
        }
        catch (RecaptchaValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> Google(GoogleLoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.GoogleLoginAsync(dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(dto, cancellationToken);
        return result is null ? NotFound(new { message = "Bu e-posta ile kayıtlı aktif bir kullanıcı bulunamadı." }) : Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var success = await _authService.ResetPasswordAsync(dto, cancellationToken);
        return success ? NoContent() : BadRequest(new { message = "Sıfırlama kodu geçersiz veya süresi dolmuş." });
    }
}
