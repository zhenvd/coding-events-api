using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text.Json.Serialization;
using CodingEventsAPI.Controllers;

namespace CodingEventsAPI.Models {
  // join table entity for relation between Tag and CodingEvent
  public class CodingEventTag {
    public long TagId { get; set; }
    public Tag Tag { get; set; }

    public long CodingEventId { get; set; }
    public CodingEvent CodingEvent { get; set; }
  }

  public class Tag : UniqueEntity, IMemberRestrictedEntity<TagDto> {
    public string Name { get; set; }

    public List<CodingEventTag> CodingEventTags { get; set; }

    public TagDto ToPublicDto() => TagDto.ForPublic(this);

    public TagDto ToMemberDto(Member requestingMember) => requestingMember.Role switch {
      MemberRole.Owner => TagDto.ForOwner(this, requestingMember),
      MemberRole.Member => TagDto.ForMember(this),
      _ => null
    };
  }

  public class NewTagDto {
    [NotNull]
    [Required]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Tag must be between 1 and 20 characters")]
    public string Name { get; set; }
  }

  public class TagDto {
    public string Name { get; set; }
    public dynamic Links { get; }

    private TagDto(Tag tag) {
      Name = tag.Name;

      Links = new ExpandoObject();
      Links.codingEvents = TagsController.ResourceLinks.GetCodingEvents(tag);
    }

    internal static TagDto ForPublic(Tag tag) {
      return new TagDto(tag);
    }

    internal static TagDto ForOwner(Tag tag, Member owner) {
      var memberTagDto = new TagDto(tag);

      memberTagDto.Links.addToCodingEvent =
        CodingEventsController.ResourceLinks.AddTag(owner.CodingEvent, tag);

      memberTagDto.Links.removeFromCodingEvent =
        CodingEventsController.ResourceLinks.RemoveTag(owner.CodingEvent, tag);

      return memberTagDto;
    }

    internal static TagDto ForMember(Tag tag) {
      return new TagDto(tag);
    }
  }
}
