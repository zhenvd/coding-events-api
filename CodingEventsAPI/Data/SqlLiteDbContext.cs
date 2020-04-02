using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Data {
  public class SqlLiteDbContext : DbContext {
    public DbSet<CodingEvent> CodingEvents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
      options.UseSqlite("Filename=sqlite.db");
    }
  }
}
