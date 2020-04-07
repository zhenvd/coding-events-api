using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Data {
  public class CodingEventsDbContext : DbContext {
    public DbSet<CodingEvent> CodingEvents { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<CodingEventTag> CodingEventTags { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Member> Members { get; set; }

    public CodingEventsDbContext(DbContextOptions options)
      : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      // -- Tag --
      // unique constraint index on Tag.Name
      modelBuilder.Entity<Tag>().HasIndex(tag => tag.Name).IsUnique();

      // -- CodingEventTag --
      // composite primary key for join table
      modelBuilder.Entity<CodingEventTag>()
        .HasKey(ceTag => new { ceTag.CodingEventId, ceTag.TagId });

      modelBuilder.Entity<CodingEventTag>()
        .HasOne(ceTag => ceTag.Tag)
        .WithMany(tag => tag.CodingEventTags)
        .HasForeignKey(ceTag => ceTag.TagId);

      modelBuilder.Entity<CodingEventTag>()
        .HasOne(ceTag => ceTag.CodingEvent)
        .WithMany(codingEvent => codingEvent.CodingEventTags)
        .HasForeignKey(ceTag => ceTag.CodingEventId);

      // -- User --
      modelBuilder.Entity<User>().HasIndex(member => member.AzureOId).IsUnique();

      modelBuilder.Entity<User>().HasIndex(member => member.Username).IsUnique();

      modelBuilder.Entity<User>().HasIndex(member => member.Email).IsUnique();

      // -- Member --
      modelBuilder.Entity<Member>().HasKey(member => member.Id);

      modelBuilder.Entity<Member>()
        .HasIndex(
          member => new {
            member.UserId, member.CodingEventId
          }
        )
        .IsUnique();

      modelBuilder.Entity<Member>()
        .HasOne(member => member.User)
        .WithMany(user => user.Memberships)
        .HasForeignKey(member => member.UserId);

      modelBuilder.Entity<Member>()
        .HasOne(member => member.CodingEvent)
        .WithMany(codingEvent => codingEvent.Members)
        .HasForeignKey(member => member.CodingEventId);
    }
  }
}
