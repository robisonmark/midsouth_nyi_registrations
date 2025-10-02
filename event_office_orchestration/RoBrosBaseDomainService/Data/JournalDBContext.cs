using Microsoft.EntityFrameworkCore;
using RoBrosBaseDomainService.Models;

namespace RoBrosBaseDomainService.Data;

public class JournalDbContext : DbContext
{
    public JournalDbContext(DbContextOptions<JournalDbContext> options) : base(options)
    {
    }

    public DbSet<EntityJournal> EntityJournals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EntityJournal>(entity =>
        {
            entity.HasIndex(e => new { e.EntityId, e.EntityType, e.CreatedAt })
                .HasDatabaseName("idx_entity_journals_entity_lookup");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_entity_journals_created_at");

            entity.HasIndex(e => new { e.EntityId, e.Version })
                .HasDatabaseName("idx_entity_journals_entity_version");
        });
    }
}