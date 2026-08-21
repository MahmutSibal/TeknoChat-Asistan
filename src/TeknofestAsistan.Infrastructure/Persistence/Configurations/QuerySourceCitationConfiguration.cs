using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class QuerySourceCitationConfiguration : IEntityTypeConfiguration<QuerySourceCitation>
{
    public void Configure(EntityTypeBuilder<QuerySourceCitation> builder)
    {
        builder.HasOne(c => c.ChatQuery)
            .WithMany(q => q.Citations)
            .HasForeignKey(c => c.ChatQueryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.SourceDocument)
            .WithMany(d => d.Citations)
            .HasForeignKey(c => c.SourceDocumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
