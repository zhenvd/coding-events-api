using System;
using System.Dynamic;
using CodingEventsAPI.Controllers;

namespace CodingEventsAPI.Models {
  public enum MemberRole {
    Owner,
    Member
  }

  public class Member : UniqueEntity {
    public Member() { }

    private Member(CodingEvent codeEvent, User user, MemberRole role) {
      CodingEvent = codeEvent;
      User = user;
      Role = role;
    }

    private Member(long codeEventId, long userId, MemberRole role) {
      CodingEventId = codeEventId;
      UserId = userId;
      Role = role;
    }

    public static Member CreateEventOwner(CodingEvent codeEvent, User member) {
      return new Member(codeEvent, member, MemberRole.Owner);
    }

    public static Member CreateEventMember(long codeEventId, long userId) {
      return new Member(codeEventId, userId, MemberRole.Member);
    }

    public MemberRole Role { get; set; }

    public long UserId { get; set; }
    public User User { get; set; }

    public long CodingEventId { get; set; }
    public CodingEvent CodingEvent { get; set; }

    public MemberDto ToDto(Member requestingMember) {
      return requestingMember.Role switch {
        MemberRole.Member => MemberDto.ForMember(this),
        MemberRole.Owner => MemberDto.ForOwner(this, requestingMember),
        _ => null
      };
    }
  }

  public class MemberDto {
    internal MemberDto() { }

    private MemberDto(Member member) {
      Email = null;
      Links = new ExpandoObject();
      Username = member.User.Username;
      Role = Enum.GetName(typeof(MemberRole), member.Role);
    }

    public static MemberDto ForMember(Member member) => new MemberDto(member);

    public static MemberDto ForOwner(Member member, Member owner) {
      var memberDtoBase = new MemberDto(member);
      memberDtoBase.Email = member.User.Email;

      if (member.Id != owner.Id) {
        memberDtoBase.Links.remove = MembersController.ResourceLinks.RemoveMember(member);
      }

      return memberDtoBase;
    }

    public string Role { get; internal set; }
    public string Username { get; internal set; }
    public string Email { get; internal set; }
    public dynamic Links { get; internal set; }
  }
}
