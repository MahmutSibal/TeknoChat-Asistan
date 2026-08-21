using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class ChatQueryConfiguration : IEntityTypeConfiguration<ChatQuery>
{
    public void Configure(EntityTypeBuilder<ChatQuery> builder)
    {
        builder.Property(q => q.QuestionText).IsRequired();

        builder.HasOne(q => q.Competition)
            .WithMany(c => c.ChatQueries)
            .HasForeignKey(q => q.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Category)
            .WithMany(c => c.ChatQueries)
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => new { q.CompetitionId, q.CreatedAt });
    }
}
