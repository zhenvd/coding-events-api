using System.Collections.Generic;
using CodingEventsAPI.Models;
using CodingEventsAPI.Services;
using CodingEventsAPI.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CodingEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route("/api/events/{codingEventId}/members")]
  public class MembersController : ControllerBase {
    private readonly IOwnerService _ownerService;
    private readonly IMemberService _memberService;
    private readonly IAuthedUserService _authedUserService;

    public static readonly MemberResourceLinks ResourceLinks =
      // the entrypoint is still /api/events (from CodingEventsController)
      // the /{codingEventId}/members subpath is inserted dynamically in the MemberResourceLinks
      new MemberResourceLinks(CodingEventsController.Entrypoint);

    public MembersController(
      IOwnerService ownerService,
      IMemberService memberService,
      IAuthedUserService authedUserService
    ) {
      _ownerService = ownerService;
      _memberService = memberService;
      _authedUserService = authedUserService;
    }

    [HttpGet]
    [SwaggerOperation(
      OperationId = "GetMembers",
      Summary = "Retrieve Members of the Coding Event",
      Description = @"
Requires an authenticated Member.
The Member data will differ depending on the requesting Member's Role
",
      Tags = new[] { SwaggerTags.RequireMemberOrOwnerTag }
    )]
    [SwaggerResponse(
      200,
      @"
List of Members with data dependent on the requesting Member's Role.<br>
Member Role: limited data (username and role).<br>
Owner Role: complete data (username, role, email, and links.remove)
",
      Type = typeof(List<MemberDto>)
    )]
    [SwaggerResponse(403, "Not a Member of the Coding Event", Type = null)]
    public ActionResult GetMembers(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId
    ) {
      var userIsMember = _memberService.IsUserAMember(codingEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      return Ok(_ownerService.GetMembersList(codingEventId, HttpContext.User));
    }

    [HttpPost]
    [SwaggerOperation(
      OperationId = "JoinCodingEvent",
      Summary = "Join a Coding Event",
      Description = "Requires an authenticated User",
      Tags = new[] { SwaggerTags.RequiredAuthedUser, }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(400, "User is already a Member or Owner", Type = null)]
    public ActionResult JoinEvent(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId
    ) {
      var userCanRegister = _memberService.CanUserRegisterAsMember(codingEventId, HttpContext.User);

      if (!userCanRegister) return BadRequest();

      _authedUserService.JoinCodingEvent(codingEventId, HttpContext.User);

      return NoContent();
    }

    [HttpDelete]
    [SwaggerOperation(
      OperationId = "LeaveCodingEvent",
      Summary = "Leave a Coding Event",
      Description = "Requires an authenticated Member",
      Tags = new[] { SwaggerTags.RequireMemberOrOwnerTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(400, "User is an Owner. (See CancelCodingEvent)", Type = null)]
    [SwaggerResponse(403, "User is not a Member", Type = null)]
    public ActionResult LeaveCodingEvent(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId
    ) {
      var userIsMember = _memberService.IsUserAMember(codingEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      var userIsOwner = _memberService.IsUserAnOwner(codingEventId, HttpContext.User);
      if (userIsOwner) return BadRequest();

      _memberService.LeaveCodingEvent(codingEventId, HttpContext.User);

      return NoContent();
    }

    [HttpDelete]
    [Route("{memberId}")]
    [SwaggerOperation(
      OperationId = "RemoveMember",
      Summary = "Remove a Member from a Coding Event",
      Description = "Requires an authenticated Owner",
      Tags = new[] { SwaggerTags.RequireOwnerTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(403, "User is not an Owner", Type = null)]
    [SwaggerResponse(404, "Member not found", Type = null)]
    public ActionResult RemoveMember(
      [FromRoute, SwaggerParameter("The ID of the Coding Event", Required = true)]
      long codingEventId,
      [FromRoute, SwaggerParameter("The ID of the Member", Required = true)]
      long memberId
    ) {
      var isOwner = _memberService.IsUserAnOwner(codingEventId, HttpContext.User);
      if (!isOwner) return StatusCode(403);

      var memberExists = _memberService.DoesMemberExist(memberId);
      if (!memberExists) return NotFound();

      _ownerService.RemoveMember(memberId);

      return NoContent();
    }
  }
}
