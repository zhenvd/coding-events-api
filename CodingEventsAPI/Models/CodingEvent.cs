using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using CodingEventsAPI.Controllers;

namespace CodingEventsAPI.Models {
  public class CodingEvent : UniqueEntity {
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    public List<Member> Members { get; set; }
    public List<CodingEventTag> CodingEventTags { get; set; } = new List<CodingEventTag>();

    public CodingEventDto ToPublicDto() => CodingEventDto.ForPublic(this);

    public CodingEventDto ToMemberDto(Member requestingMember) {
      return requestingMember.Role switch {
        MemberRole.Owner => CodingEventDto.ForOwner(this),
        MemberRole.Member => CodingEventDto.ForMember(this),
        _ => null,
      };
    }
  }

  // a input DTO is used to prevent an "over-posting [mass assignment]" attack
  // https://cheatsheetseries.owasp.org/cheatsheets/Mass_Assignment_Cheat_Sheet.html
  public class NewCodingEventDto {
    [NotNull]
    [Required]
    [StringLength(
      100,
      MinimumLength = 10,
      ErrorMessage = "Title must be between 10 and 100 characters"
    )]
    public string Title { get; set; }

    [NotNull]
    [Required]
    [StringLength(1000, ErrorMessage = "Description can't be more than 1000 characters")]
    public string Description { get; set; }

    [Required] [NotNull] public DateTime Date { get; set; }
  }

  public class CodingEventDto {
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public dynamic Links { get; set; }
    public string Description { get; set; }

    internal CodingEventDto(CodingEvent codingEvent) {
      Title = codingEvent.Title;
      Date = codingEvent.Date;

      Links = new ExpandoObject();
      Links.codingEvent = CodingEventsController.ResourceLinks.GetCodingEvent(codingEvent);
      Links.tags = CodingEventsController.ResourceLinks.GetTags(codingEvent);
    }

    public static CodingEventDto ForPublic(CodingEvent codingEvent) {
      var codingEventDto = new CodingEventDto(codingEvent);
      codingEventDto.Links.join = MembersController.ResourceLinks.JoinCodingEvent(codingEvent);

      return codingEventDto;
    }

    public static CodingEventDto ForMember(CodingEvent codingEvent) {
      var codingEventDto = ForAnyMemberRole(codingEvent);
      codingEventDto.Links.leave = MembersController.ResourceLinks.LeaveCodingEvent(codingEvent);

      return codingEventDto;
    }

    public static CodingEventDto ForOwner(CodingEvent codingEvent) {
      var codingEventDto = ForAnyMemberRole(codingEvent);
      codingEventDto.Links.cancel =
        CodingEventsController.ResourceLinks.CancelCodingEvent(codingEvent);

      return codingEventDto;
    }

    private static CodingEventDto ForAnyMemberRole(CodingEvent codingEvent) {
      var codingEventDto = new CodingEventDto(codingEvent);
      codingEventDto.Description = codingEvent.Description;
      codingEventDto.Links.members = MembersController.ResourceLinks.GetMembers(codingEvent);

      return codingEventDto;
    }
  }
}
