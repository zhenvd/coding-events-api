using System.Collections.Generic;
using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Data.Repositories;
using CodingEventsAPI.Models;
using CodingEventsAPI.Services;
using CodingEventsAPI.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CodingEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route(Entrypoint)]
  public class CodingEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    public static readonly CodingEventResourceLinks ResourceLinks =
      new CodingEventResourceLinks(Entrypoint);

    private readonly IOwnerService _ownerService;
    private readonly IMemberService _memberService;
    private readonly IAuthedUserService _authedUserService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly ICodingEventTagService _codingEventTagService;

    public CodingEventsController(
      IOwnerService ownerService,
      IMemberService memberService,
      IAuthedUserService authedUserService,
      IPublicAccessService publicAccessService,
      ICodingEventTagService codingEventTagService
    ) {
      _ownerService = ownerService;
      _memberService = memberService;
      _authedUserService = authedUserService;
      _publicAccessService = publicAccessService;
      _codingEventTagService = codingEventTagService;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
      OperationId = "GetCodingEvents",
      Summary = "Retrieve all Coding Events",
      Description = "Publicly available",
      Tags = new[] { SwaggerTags.PublicAccessTag }
    )]
    [SwaggerResponse(200, "List of public Coding Event data", Type = typeof(List<CodingEventDto>))]
    public ActionResult GetCodingEvents() {
      return Ok(_publicAccessService.GetAllCodingEvents());
    }

    [HttpPost]
    [SwaggerOperation(
      OperationId = "CreateCodingEvent",
      Summary = "Create a new Coding Event",
      Description = "Requires an authenticated User",
      Tags = new[] { SwaggerTags.RequiredAuthedUser }
    )]
    [SwaggerResponse(201, "Returns new public Coding Event data", Type = typeof(CodingEventDto))]
    [SwaggerResponse(400, "Invalid or missing Coding Event data", Type = null)]
    public ActionResult CreateCodingEvent(
      [FromBody, SwaggerParameter("New Coding Event data", Required = true)]
      NewCodingEventDto newCodingEvent
    ) {
      var codingEvent = _authedUserService.RegisterCodingEvent(newCodingEvent, HttpContext.User);

      return CreatedAtAction(
        nameof(GetCodingEvent),
        new { codingEventId = codingEvent.Id },
        codingEvent.ToPublicDto()
      );
    }

    [HttpGet]
    [Route("{codingEventId}")]
    [SwaggerOperation(
      OperationId = "GetCodingEvent",
      Summary = "Retrieve Coding Event data",
      Description = "Requires an authenticated Member or Owner of the Coding Event",
      Tags = new[] { SwaggerTags.RequireMemberOrOwnerTag }
    )]
    [SwaggerResponse(
      200,
      @"
Complete Coding Event data with links available to the requesting Member's Role.<br>
Member Role: links.leave.<br>
Owner Role: links.cancel
",
      Type = typeof(CodingEventDto)
    )]
    [SwaggerResponse(403, "Not a Member of the Coding Event", Type = null)]
    [SwaggerResponse(404, "Coding Event not found", Type = null)]
    public ActionResult GetCodingEvent(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId
    ) {
      var userIsMember = _memberService.IsUserAMember(codingEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      var codingEventDto = _memberService.GetCodingEventById(codingEventId, HttpContext.User);
      if (codingEventDto == null) return NotFound();

      return Ok(codingEventDto);
    }

    [HttpDelete]
    [Route("{codingEventId}")]
    [SwaggerOperation(
      OperationId = "CancelCodingEvent",
      Summary = "Cancel a Coding Event",
      Description = "Requires an authenticated Owner of the Coding Event",
      Tags = new[] { SwaggerTags.RequireOwnerTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(403, "Not an Owner of the Coding Event", Type = null)]
    public ActionResult CancelCodingEvent(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId
    ) {
      var isOwner = _memberService.IsUserAnOwner(codingEventId, HttpContext.User);
      if (!isOwner) return StatusCode(403);

      _ownerService.CancelCodingEvent(codingEventId);

      return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{codingEventId}/tags")]
    [SwaggerOperation(
      OperationId = "GetCodingEventTags",
      Summary = "Get the Coding Event Tags",
      Tags = new[] { SwaggerTags.PublicAccessTag, SwaggerTags.RequireMemberOrOwnerTag }
    )]
    [SwaggerResponse(
      200,
      @"
List of Tags associated with the Coding Event<br>
Public: links.tag, links.codingEvents<br>
Owners: includes links.add and links.remove
",
      Type = typeof(List<TagDto>)
    )]
    public ActionResult GetCodingEventTags(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId
    ) {
      List<TagDto> tags;

      if (HttpContext.User.Identity.IsAuthenticated) {
        tags = _memberService.GetTagsForCodingEvent(codingEventId, HttpContext.User);
      }
      else {
        tags = _publicAccessService.GetTagsForCodingEvent(codingEventId);
      }

      return Ok(tags);
    }

    [HttpPut]
    [Route("{codingEventId}/tags/{tagId}")]
    [SwaggerOperation(
      OperationId = "AddTagToCodingEvent",
      Summary = "Add a Tag to a Coding Event",
      Tags = new[] { SwaggerTags.RequireOwnerTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(403, "Not an Owner of the Coding Event", Type = null)]
    [SwaggerResponse(
      400,
      "Tag can not be associated with the Coding Event (not found or already associated)",
      Type = null
    )]
    public ActionResult AddTagToCodingEvent(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId,
      [FromRoute, SwaggerParameter("The ID of the Tag", Required = true)]
      long tagId
    ) {
      var isOwner = _memberService.IsUserAnOwner(codingEventId, HttpContext.User);
      if (!isOwner) return StatusCode(403);

      var tagCanBeAdded = _codingEventTagService.CanTagBeAdded(codingEventId, tagId);
      if (!tagCanBeAdded) return BadRequest();

      _codingEventTagService.AddTagToCodingEvent(codingEventId, tagId);

      return NoContent();
    }

    [HttpDelete]
    [Route("{codingEventId}/tags/{tagId}")]
    [SwaggerOperation(
      OperationId = "RemoveTagFromCodingEvent",
      Summary = "Remove a Tag from a Coding Event",
      Tags = new[] { SwaggerTags.RequireOwnerTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(
      400,
      "Tag can not be removed from the Coding Event (not found or not associated)",
      Type = null
    )]
    [SwaggerResponse(403, "Not an Owner of the Coding Event", Type = null)]
    public ActionResult RemoveTagFromCodingEvent(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId,
      [FromRoute, SwaggerParameter("The ID of the Tag", Required = true)]
      long tagId
    ) {
      var isOwner = _memberService.IsUserAnOwner(codingEventId, HttpContext.User);
      if (!isOwner) return StatusCode(403);

      var tagCanBeRemoved = _codingEventTagService.CanTagBeRemoved(codingEventId, tagId);
      if (!tagCanBeRemoved) return BadRequest();

      _codingEventTagService.RemoveTagFromCodingEvent(codingEventId, tagId);

      return NoContent();
    }
  }
}
