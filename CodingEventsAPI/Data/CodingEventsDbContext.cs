using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Data {
  public class CodingEventsDbContext : DbContext {
    public DbSet<CodingEvent> CodingEvents { get; set; }

    public CodingEventsDbContext(DbContextOptions options)
      : base(options) { }
  }
}
