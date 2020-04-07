using System.Collections.Generic;
using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Data.Repositories;
using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Services {
  public interface IPublicAccessService {
    List<CodingEventDto> GetAllCodingEvents();

    Tag CreateTag(NewTagDto newTagDto);
    List<TagDto> GetTags();
    TagDto GetTagById(long tagId);
    List<TagDto> GetTagsForCodingEvent(long codingEventId);
    List<CodingEventDto> GetCodingEventsForTag(long tagId);
  }

  public class PublicAccessService : IPublicAccessService {
    private readonly CodingEventsDbContext _dbContext;
    private readonly ICodingEventRepository _codingEventRepository;
    private readonly ITagRepository _tagRepository;

    public PublicAccessService(
      CodingEventsDbContext dbContext,
      ICodingEventRepository codingEventRepository,
      ITagRepository tagRepository
    ) {
      _dbContext = dbContext;
      _codingEventRepository = codingEventRepository;
      _tagRepository = tagRepository;
    }

    public List<CodingEventDto> GetAllCodingEvents() => _codingEventRepository.GetAllCodingEvents()
      .Select(ce => ce.ToPublicDto())
      .ToList();

    public Tag CreateTag(NewTagDto newTagDto) {
      if (_tagRepository.Exists(newTagDto.Name)) return null;

      var tagEntry = _dbContext.Tags.Add(new Tag());
      tagEntry.CurrentValues.SetValues(newTagDto);

      _dbContext.SaveChanges();

      var newTag = tagEntry.Entity;
      return newTag;
    }

    public List<TagDto> GetTags() =>
      _tagRepository.GetTags().Select(tag => tag.ToPublicDto()).ToList();

    public TagDto GetTagById(long tagId) {
      return _tagRepository.GetTagById(tagId).ToPublicDto();
    }

    public List<TagDto> GetTagsForCodingEvent(long codingEventId) {
      return _codingEventRepository.GetTagsForCodingEvent(codingEventId)
        .Select(tag => tag.ToPublicDto())
        .ToList();
    }

    public List<CodingEventDto> GetCodingEventsForTag(long tagId) {
      var tag = _dbContext.Tags.Include(t => t.CodingEventTags)
        .ThenInclude(ceTag => ceTag.CodingEvent)
        .SingleOrDefault(t => t.Id == tagId);
      
      return tag == null ? null : tag.CodingEventTags.Select(ceTag => ceTag.CodingEvent.ToPublicDto()).ToList();
    }
  }
}
