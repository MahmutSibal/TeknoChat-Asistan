using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class FaqEntryConfiguration : IEntityTypeConfiguration<FaqEntry>
{
    public void Configure(EntityTypeBuilder<FaqEntry> builder)
    {
        builder.Property(f => f.Question).IsRequired();
        builder.Property(f => f.Answer).IsRequired();

        builder.HasOne(f => f.Competition)
            .WithMany(c => c.FaqEntries)
            .HasForeignKey(f => f.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Category)
            .WithMany(c => c.FaqEntries)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.SourceChatQuery)
            .WithMany()
            .HasForeignKey(f => f.SourceChatQueryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.CreatedByUser)
            .WithMany()
            .HasForeignKey(f => f.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
