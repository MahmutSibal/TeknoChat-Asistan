using TeknofestAsistan.Domain.Common;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    /// <summary>False only for self-registered accounts pending email verification. Admin-provisioned
    /// and Google-authenticated accounts are trusted immediately.</summary>
    public bool IsEmailVerified { get; set; } = true;
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeExpiresAt { get; set; }

    public ICollection<SourceDocument> UploadedDocuments { get; set; } = new List<SourceDocument>();
    public ICollection<SupportTicket> AssignedTickets { get; set; } = new List<SupportTicket>();
}
