using System.Collections.Generic;
using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CodingEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route(Entrypoint)]
  public class TagsController : ControllerBase {
    public const string Entrypoint = "/api/tags";

    private CodingEventsDbContext _dbContext;

    public TagsController(CodingEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
      OperationId = "GetTags",
      Summary = "Retrieve all Tags",
      Description = "Publicly available"
    )]
    [SwaggerResponse(200, "List of Tag data", Type = typeof(List<Tag>))]
    public ActionResult GetTags() => Ok(_dbContext.Tags.ToList());

    [HttpGet]
    [Route("{tagId}")]
    [SwaggerOperation(OperationId = "GetTag", Summary = "Retrieve Tag data")]
    [SwaggerResponse(200, "Tag data", Type = typeof(Tag))]
    [SwaggerResponse(404, "Tag not found", Type = null)]
    public ActionResult GetTag([FromRoute] long tagId) {
      var tag = _dbContext.Tags.Find(tagId);
      if (tag == null) return NotFound();

      return Ok(tag);
    }


    [HttpPost]
    [SwaggerOperation(OperationId = "CreateTag", Summary = "Create a new Tag")]
    [SwaggerResponse(201, "Returns new Tag data", Type = typeof(Tag))]
    [SwaggerResponse(400, "Invalid or missing Tag data", Type = null)]
    public ActionResult CreateTag([FromBody] NewTagDto newTagDto) {
      var tagEntry = _dbContext.Tags.Add(new Tag());
      tagEntry.CurrentValues.SetValues(newTagDto);

      try {
        _dbContext.SaveChanges();
      }
      catch (DbUpdateException) {
        // unique Tag.Name violation
        return BadRequest();
      }

      var newTag = tagEntry.Entity;

      return CreatedAtAction(nameof(GetTag), new { tagId = newTag.Id }, newTag);
    }

    [HttpGet]
    [Route("{tagId}/events")]
    [SwaggerOperation(
      OperationId = "GetTagEvents",
      Summary = "Retrieve Coding Events with the given Tag"
    )]
    [SwaggerResponse(200, "Coding Events", Type = typeof(List<CodingEvent>))]
    [SwaggerResponse(404, "Tag not found", Type = null)]
    public ActionResult GetTagEvents([FromRoute] long tagId) {
      var tag = _dbContext.Tags.Include(t => t.CodingEventTags)
        .ThenInclude(ceTag => ceTag.CodingEvent)
        .SingleOrDefault(t => t.Id == tagId);
      if (tag == null) return NotFound();

      return Ok(tag.CodingEventTags.Select(ceTag => ceTag.CodingEvent));
    }
  }
}
