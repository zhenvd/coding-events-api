using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using CodingEventsAPI.Data;
using CodingEventsAPI.Data.Repositories;
using CodingEventsAPI.Models;

namespace CodingEventsAPI.Services {
  public interface IMemberService {
// S:U    
    bool CanUserRegisterAsMember(long codeEventId, ClaimsPrincipal authedUser);

// S:M
    bool IsUserAMember(long codeEventId, ClaimsPrincipal authedUser);

    bool IsUserAnOwner(long codeEventId, ClaimsPrincipal authedUser);

// R:M
    bool DoesMemberExist(long memberId);

    CodingEventDto GetCodingEventById(long codingEventId, ClaimsPrincipal authedUser);

    List<TagDto> GetTagsForCodingEvent(long codingEventId, ClaimsPrincipal authedUser);

    void LeaveCodingEvent(long codingEventId, ClaimsPrincipal authedUser);
  }

  public class MemberService : IMemberService {
    private readonly CodingEventsDbContext _dbContext;
    private readonly IAuthedUserService _authedUserService;
    private readonly ICodingEventRepository _codingEventRepository;

    public MemberService(
      CodingEventsDbContext dbContext,
      IAuthedUserService authedUserService,
      ICodingEventRepository codingEventRepository
    ) {
      _dbContext = dbContext;
      _authedUserService = authedUserService;
      _codingEventRepository = codingEventRepository;
    }

    public CodingEventDto GetCodingEventById(long codingEventId, ClaimsPrincipal authedUser) {
      var requestingMember =
        _authedUserService.ConvertAuthedUserToMember(codingEventId, authedUser);

      return _dbContext.CodingEvents.Find(codingEventId)?.ToMemberDto(requestingMember);
    }

    public List<TagDto> GetTagsForCodingEvent(long codingEventId, ClaimsPrincipal authedUser) {
      var requestingMember =
        _authedUserService.ConvertAuthedUserToMember(codingEventId, authedUser);

      return _codingEventRepository.GetTagsForCodingEvent(codingEventId)
        .Select(tag => tag.ToMemberDto(requestingMember))
        .ToList();
    }

    public bool CanUserRegisterAsMember(long codeEventId, ClaimsPrincipal authedUser) {
      var isMember = IsUserAMember(codeEventId, authedUser);

      return !isMember;
    }

    public bool IsUserAMember(long codeEventId, ClaimsPrincipal authedUser) {
      var codeEventCount = _dbContext.CodingEvents.Count(ce => ce.Id == codeEventId);
      var codeEventExists = codeEventCount == 1;
      if (!codeEventExists) return false;

      var user = _authedUserService.ConvertAuthedUserToUser(authedUser);
      var memberCount = _dbContext.Members.Count(
        m => m.UserId == user.Id && m.CodingEventId == codeEventId
      );
      var isMember = memberCount == 1;

      return isMember;
    }

    public bool IsUserAnOwner(long codeEventId, ClaimsPrincipal authedUser) {
      var isMember = IsUserAMember(codeEventId, authedUser);
      if (!isMember) return false;

      var requestingMember = _authedUserService.ConvertAuthedUserToMember(codeEventId, authedUser);

      return requestingMember.Role == MemberRole.Owner;
    }

    public bool DoesMemberExist(long memberId) {
      var memberCount = _dbContext.Members.Count(m => m.Id == memberId);
      var memberExists = memberCount == 1;

      return memberExists;
    }

    public void LeaveCodingEvent(long codingEventId, ClaimsPrincipal authedUser) {
      var leavingMember = _authedUserService.ConvertAuthedUserToMember(codingEventId, authedUser);

      _dbContext.Members.Remove(leavingMember);
      _dbContext.SaveChanges();
    }
  }
}
