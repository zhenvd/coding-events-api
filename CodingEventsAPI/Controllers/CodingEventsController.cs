using System.Collections.Generic;
using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CodingEventsAPI.Controllers {
  [Authorize] // all endpoints in controller require Authentication/Authorization unless specified
  [ApiController]
  [Route(Entrypoint)]
  public class CodingEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    private readonly CodingEventsDbContext _dbContext;

    public CodingEventsController(CodingEventsDbContext dbContext) {
      _dbContext = dbContext;
    }


    [HttpGet]
    [AllowAnonymous] // does not require Authentication / Authorization
    [SwaggerOperation(
      OperationId = "GetCodingEvents",
      Summary = "Retrieve all Coding Events",
      Description = "Publicly available"
    )]
    [SwaggerResponse(200, "List of public Coding Event data", Type = typeof(List<CodingEvent>))]
    public ActionResult GetCodingEvents() => Ok(_dbContext.CodingEvents.ToList());

    [HttpPost]
    [SwaggerOperation(OperationId = "RegisterCodingEvent", Summary = "Create a new Coding Event")]
    [SwaggerResponse(201, "Returns new Coding Event data", Type = typeof(CodingEvent))]
    [SwaggerResponse(400, "Invalid or missing Coding Event data", Type = null)]
    public ActionResult RegisterCodingEvent([FromBody] NewCodingEventDto newCodingEventDto) {
      var codingEventEntry = _dbContext.CodingEvents.Add(new CodingEvent());
      codingEventEntry.CurrentValues.SetValues(newCodingEventDto);
      _dbContext.SaveChanges();

      var newCodingEvent = codingEventEntry.Entity;

      return CreatedAtAction(
        nameof(GetCodingEvent),
        new { codingEventId = newCodingEvent.Id },
        newCodingEvent
      );
    }

    [HttpGet]
    [Route("{codingEventId}")]
    [SwaggerOperation(OperationId = "GetCodingEvent", Summary = "Retrieve Coding Event data")]
    [SwaggerResponse(200, "Complete Coding Event data", Type = typeof(CodingEvent))]
    [SwaggerResponse(404, "Coding Event not found", Type = null)]
    public ActionResult GetCodingEvent([FromRoute] long codingEventId) {
      var codingEvent = _dbContext.CodingEvents.Find(codingEventId);
      if (codingEvent == null) return NotFound();

      return Ok(codingEvent);
    }

    [HttpDelete]
    [Route("{codingEventId}")]
    [SwaggerOperation(
      OperationId = "CancelCodingEvent",
      Summary = "Cancel (delete) a Coding Event"
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    public ActionResult CancelCodingEvent([FromRoute] long codingEventId) {
      _dbContext.CodingEvents.Remove(new CodingEvent { Id = codingEventId });

      try {
        _dbContext.SaveChanges();
      }
      catch (DbUpdateConcurrencyException) {
        // row did not exist
        return NotFound();
      }

      return NoContent();
    }

    [HttpPut]
    [Route("{codingEventId}/tags/{tagId}")]
    [SwaggerOperation(OperationId = "AddTagToCodingEvent", Summary = "Add a Tag to a Coding Event")]
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(404, "Coding Event or Tag not found", Type = null)]
    public ActionResult AddTagToCodingEvent(
      [FromRoute] long codingEventId,
      [FromRoute] long tagId
    ) {
      var codingEvent = _dbContext.CodingEvents.Find(codingEventId);
      if (codingEvent == null) return NotFound();

      var tag = _dbContext.Tags.Find(tagId);
      // var tag = _dbContext.Tags.Find(addTagDto.TagId);
      if (tag == null) return NotFound();

      codingEvent.CodingEventTags.Add(new CodingEventTag { Tag = tag, CodingEvent = codingEvent });

      try {
        _dbContext.SaveChanges();
      }
      catch (DbUpdateException) {
        // duplicate tags
        return BadRequest();
      }

      return NoContent();
    }

    [HttpDelete]
    [Route("{codingEventId}/tags/{tagId}")]
    [SwaggerOperation(
      OperationId = "RemoveTagFromCodingEvent",
      Summary = "Remove a Tag from a Coding Event"
    )]
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(404, "Coding Event or Tag not found", Type = null)]
    public ActionResult RemoveTagFromCodingEvent(
      [FromRoute] long codingEventId,
      [FromRoute] long tagId
    ) {
      var codingEvent = _dbContext.CodingEvents.Include(ce => ce.CodingEventTags)
        .SingleOrDefault(ce => ce.Id == codingEventId);
      if (codingEvent == null) return NotFound();

      var tag = codingEvent.CodingEventTags.Find(ceTag => ceTag.TagId == tagId);
      if (tag == null) return BadRequest();

      codingEvent.CodingEventTags.Remove(tag);
      _dbContext.SaveChanges();

      return NoContent();
    }
  }
}
