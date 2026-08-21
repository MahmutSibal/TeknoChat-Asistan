using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(c => c.Competition)
            .WithMany(comp => comp.Categories)
            .HasForeignKey(c => c.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
