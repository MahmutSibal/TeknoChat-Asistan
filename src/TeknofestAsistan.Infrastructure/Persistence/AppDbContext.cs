using Microsoft.EntityFrameworkCore;
using TeknofestAsistan.Domain.Entities;

namespace TeknofestAsistan.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<SourceDocument> SourceDocuments => Set<SourceDocument>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<ChatQuery> ChatQueries => Set<ChatQuery>();
    public DbSet<QuerySourceCitation> QuerySourceCitations => Set<QuerySourceCitation>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<FaqEntry> FaqEntries => Set<FaqEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
