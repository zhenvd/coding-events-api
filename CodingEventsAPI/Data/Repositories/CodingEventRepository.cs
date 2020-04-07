using System.Collections.Generic;
using System.Linq;
using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Data.Repositories {
  public interface ICodingEventRepository : IBaseRepository<CodingEvent> {
    IEnumerable<CodingEvent> GetAllCodingEvents();
    void CancelCodingEvent(long codingEventId);

    IEnumerable<Tag> GetTagsForCodingEvent(long codingEventId);
  }

  public class CodingEventRepository : ICodingEventRepository {
    private readonly CodingEventsDbContext _dbContext;

    public CodingEventRepository(CodingEventsDbContext dbContext) {
      _dbContext = dbContext;
    }


    public bool Exists(long id) {
      var codingEventCount = _dbContext.CodingEvents.Count(ce => ce.Id == id);

      return codingEventCount == 1;
    }

    public IEnumerable<CodingEvent> GetAllCodingEvents() {
      return _dbContext.CodingEvents.ToList();
    }

    public void CancelCodingEvent(long codingEventId) {
      var codingEventProxy = new CodingEvent() { Id = codingEventId };
      _dbContext.CodingEvents.Remove(codingEventProxy);
      _dbContext.SaveChanges();
    }

    public IEnumerable<Tag> GetTagsForCodingEvent(long codingEventId) {
      var codingEvent = _dbContext.CodingEvents.Include(ce => ce.CodingEventTags)
        .ThenInclude(ceTag => ceTag.Tag)
        .SingleOrDefault(ce => ce.Id == codingEventId);

      return codingEvent?.CodingEventTags.Select(ceTag => ceTag.Tag).ToList();
    }
  }
}
