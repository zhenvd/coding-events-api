using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodingEventsAPI.Controllers {
  [ApiController]
  [Route(Entrypoint)]
  public class CodingEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    private readonly CodingEventsDbContext _dbContext;

    public CodingEventsController(CodingEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    [HttpGet]
    public ActionResult GetCodingEvents() => Ok(_dbContext.CodingEvents.ToList());

    [HttpPost]
    public ActionResult RegisterCodingEvent(NewCodingEventDto newCodingEventDto) {
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
    public ActionResult GetCodingEvent(long codingEventId) {
      var codingEvent = _dbContext.CodingEvents.Find(codingEventId);
      if (codingEvent == null) return NotFound();

      return Ok(codingEvent);
    }

    [HttpDelete]
    [Route("{codingEventId}")]
    public ActionResult CancelCodingEvent(long codingEventId) {
      var codingEventProxy = new CodingEvent { Id = codingEventId };
      _dbContext.CodingEvents.Remove(codingEventProxy);
      _dbContext.SaveChanges();

      return NoContent();
    }
  }
}
