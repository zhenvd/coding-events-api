using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Data.Repositories;
using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Services {
  public interface ICodingEventTagService {
    bool CanTagBeAdded(long codingEventId, long tagId);

    bool CanTagBeRemoved(long codingEventId, long tagId);

    void AddTagToCodingEvent(long codingEventId, long tagId);

    void RemoveTagFromCodingEvent(long codingEventId, long tagId);
  }

  public class CodingEventTagService : ICodingEventTagService {
    private readonly CodingEventsDbContext _dbContext;
    private readonly ICodingEventRepository _codingEventRepository;
    private readonly ITagRepository _tagRepository;

    public CodingEventTagService(
      CodingEventsDbContext dbContext,
      ICodingEventRepository codingEventRepository,
      ITagRepository tagRepository
    ) {
      _dbContext = dbContext;
      _codingEventRepository = codingEventRepository;
      _tagRepository = tagRepository;
    }

    private bool CodingEventAndTagExist(long codingEventId, long tagId) {
      return _codingEventRepository.Exists(codingEventId) && _tagRepository.Exists(tagId);
    }

    private bool CodingEventHasTag(long codingEventId, long tagId) {
      var codingEventTagCount = _dbContext.CodingEventTags.Count(
        ceTag => ceTag.CodingEventId == codingEventId && ceTag.TagId == tagId
      );

      return codingEventTagCount == 1;
    }

    public bool CanTagBeAdded(long codingEventId, long tagId) {
      if (!CodingEventAndTagExist(codingEventId, tagId)) {
        return false;
      }

      return !CodingEventHasTag(codingEventId, tagId);
    }

    public bool CanTagBeRemoved(long codingEventId, long tagId) {
      return CodingEventAndTagExist(codingEventId, tagId) &&
             CodingEventHasTag(codingEventId, tagId);
    }

    public void AddTagToCodingEvent(long codingEventId, long tagId) {
      var codingEvent = _dbContext.CodingEvents.Find(codingEventId);
      var tag = _dbContext.Tags.Find(tagId);

      codingEvent.CodingEventTags.Add(new CodingEventTag { Tag = tag, CodingEvent = codingEvent });

      _dbContext.SaveChanges();
    }

    public void RemoveTagFromCodingEvent(long codingEventId, long tagId) {
      var codingEventTagProxy = new CodingEventTag { CodingEventId = codingEventId, TagId = tagId };
      _dbContext.CodingEventTags.Remove(codingEventTagProxy);
      _dbContext.SaveChanges();
    }
  }
}
