using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Data {
  public class CodingEventsDbContext : DbContext {
    public DbSet<CodingEvent> CodingEvents { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public CodingEventsDbContext(DbContextOptions options)
      : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      // unique constraint index on Tag.Name
      modelBuilder.Entity<Tag>().HasIndex(tag => tag.Name).IsUnique();

      // composite primary key for join table
      modelBuilder.Entity<CodingEventTag>()
        .HasKey(ceTag => new { ceTag.TagId, ceTag.CodingEventId });

      modelBuilder.Entity<CodingEventTag>()
        .HasOne(ceTag => ceTag.Tag)
        .WithMany(tag => tag.CodingEventTags)
        .HasForeignKey(ceTag => ceTag.TagId);

      modelBuilder.Entity<CodingEventTag>()
        .HasOne(ceTag => ceTag.CodingEvent)
        .WithMany(codingEvent => codingEvent.CodingEventTags)
        .HasForeignKey(ceTag => ceTag.CodingEventId);
    }
  }
}
