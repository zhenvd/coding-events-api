using System.Collections.Generic;
using CodingEventsAPI.Models;
using CodingEventsAPI.Services;
using CodingEventsAPI.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CodingEventsAPI.Controllers {
  [ApiController]
  [Route(Entrypoint)]
  public class TagsController : ControllerBase {
    public const string Entrypoint = "/api/tags";

    public static readonly TagResourceLinks ResourceLinks = new TagResourceLinks(Entrypoint);

    private readonly IPublicAccessService _publicAccessService;

    public TagsController(IPublicAccessService publicAccessService) {
      _publicAccessService = publicAccessService;
    }

    [HttpGet]
    [SwaggerOperation(
      OperationId = "GetTags",
      Summary = "Retrieve all Tags",
      Description = "Publicly available",
      Tags = new[] { SwaggerTags.PublicAccessTag }
    )]
    [SwaggerResponse(200, "List of Tag data", Type = typeof(List<TagDto>))]
    public ActionResult GetTags() => Ok(_publicAccessService.GetTags());

    [HttpGet]
    [Route("{tagId}")]
    [SwaggerOperation(
      OperationId = "GetTag",
      Summary = "Retrieve Tag data",
      Tags = new[] { SwaggerTags.PublicAccessTag }
    )]
    [SwaggerResponse(200, "Tag data", Type = typeof(TagDto))]
    [SwaggerResponse(404, "Tag not found", Type = null)]
    public ActionResult GetTag([FromRoute] long tagId) {
      var tagDto = _publicAccessService.GetTagById(tagId);
      if (tagDto == null) return NotFound();

      return Ok(tagDto);
    }


    [HttpPost]
    [Authorize]
    [SwaggerOperation(
      OperationId = "CreateTag",
      Summary = "Create a new Tag",
      Tags = new[] { SwaggerTags.RequiredAuthedUser }
    )]
    [SwaggerResponse(201, "Returns new public Tag data", Type = typeof(TagDto))]
    [SwaggerResponse(400, "Invalid or missing Tag data", Type = null)]
    public ActionResult CreateTag(
      [FromBody, SwaggerParameter("New Tag data", Required = true)]
      NewTagDto newTagDto
    ) {
      var newTag = _publicAccessService.CreateTag(newTagDto);
      if (newTag == null) return BadRequest();

      return CreatedAtAction(nameof(GetTag), new { tagId = newTag.Id }, newTag.ToPublicDto());
    }

    [HttpGet]
    [Route("{tagId}/events")]
    [SwaggerOperation(
      OperationId = "GetTagEvents",
      Summary = "Retrieve Coding Events with the given Tag",
      Tags = new[] { SwaggerTags.PublicAccessTag }
    )]
    [SwaggerResponse(200, "Coding Events", Type = typeof(List<CodingEventDto>))]
    [SwaggerResponse(404, "Tag not found", Type = null)]
    public ActionResult GetCodingEventsForTag([FromRoute] long tagId) {
      var codingEventDtos = _publicAccessService.GetCodingEventsForTag(tagId);
      if (codingEventDtos == null) return NotFound();

      return Ok(codingEventDtos);
    }
  }
}
