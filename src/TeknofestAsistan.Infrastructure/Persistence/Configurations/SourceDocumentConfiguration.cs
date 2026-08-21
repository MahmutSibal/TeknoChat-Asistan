using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class SourceDocumentConfiguration : IEntityTypeConfiguration<SourceDocument>
{
    public void Configure(EntityTypeBuilder<SourceDocument> builder)
    {
        builder.Property(d => d.Title).IsRequired().HasMaxLength(300);
        builder.Property(d => d.FileName).HasMaxLength(300);
        builder.Property(d => d.FilePath).HasMaxLength(1000);

        builder.HasOne(d => d.Competition)
            .WithMany(c => c.SourceDocuments)
            .HasForeignKey(d => d.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Category)
            .WithMany(c => c.SourceDocuments)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.UploadedByUser)
            .WithMany(u => u.UploadedDocuments)
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.CompetitionId, d.CategoryId, d.IsActive });
    }
}
