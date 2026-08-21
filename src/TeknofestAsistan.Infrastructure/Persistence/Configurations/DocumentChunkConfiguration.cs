using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.Property(c => c.Content).IsRequired();

        builder.HasOne(c => c.SourceDocument)
            .WithMany(d => d.Chunks)
            .HasForeignKey(c => c.SourceDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
