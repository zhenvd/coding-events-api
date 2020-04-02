using Microsoft.AspNetCore.Mvc;

namespace CodingEventsAPI.Controllers {
  [ApiController]
  [Route(Entrypoint)]
  public class CodingEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    [HttpGet]
    public ActionResult GetCodingEvents() {
      return Ok();
    }
  }
}
