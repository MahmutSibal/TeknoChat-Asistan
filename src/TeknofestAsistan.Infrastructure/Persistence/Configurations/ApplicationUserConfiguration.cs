using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        // Default keeps the AddColumn migration valid against any pre-existing rows; every
        // code path that creates a user always supplies a real hash going forward.
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500).HasDefaultValue(string.Empty);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(64);
        // Existing rows (and admin-provisioned/Google accounts) must stay usable — only the
        // self-registration code path explicitly sets this false.
        builder.Property(u => u.IsEmailVerified).HasDefaultValue(true);
        builder.Property(u => u.EmailVerificationCode).HasMaxLength(6);
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
