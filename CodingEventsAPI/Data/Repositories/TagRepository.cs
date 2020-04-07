using System.Collections.Generic;
using System.Linq;
using CodingEventsAPI.Models;

namespace CodingEventsAPI.Data.Repositories {
  public interface ITagRepository : IBaseRepository<Tag> {
    IEnumerable<Tag> GetTags();
    Tag GetTagById(long tagId);
    bool Exists(string name);
  }

  public class TagRepository : ITagRepository {
    private readonly CodingEventsDbContext _dbContext;

    public TagRepository(CodingEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    public bool Exists(long entityId) {
      var tagCount = _dbContext.Tags.Count(ce => ce.Id == entityId);

      return tagCount == 1;
    }

    public bool Exists(string name) {
      var tagCount = _dbContext.Tags.Count(ce => ce.Name == name);

      return tagCount == 1;
    }

    public IEnumerable<Tag> GetTags() => _dbContext.Tags.ToList();

    public Tag GetTagById(long tagId) {
      return _dbContext.Tags.Find(tagId);
    }
  }
}
