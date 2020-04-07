using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CodingEventsAPI.Data;
using CodingEventsAPI.Data.Repositories;
using CodingEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingEventsAPI.Services {
  public interface IOwnerService {
    void CancelCodingEvent(long codingEventId);

    List<MemberDto> GetMembersList(long codingEventId, ClaimsPrincipal authedUser);

    void RemoveMember(long memberId);
  }

  public class OwnerService : IOwnerService {
    private readonly CodingEventsDbContext _dbContext;
    private readonly IAuthedUserService _authedUserService;
    private readonly ICodingEventRepository _codingEventRepository;

    public OwnerService(
      CodingEventsDbContext dbContext,
      IAuthedUserService authedUserService,
      ICodingEventRepository codingEventRepository
    ) {
      _dbContext = dbContext;
      _authedUserService = authedUserService;
      _codingEventRepository = codingEventRepository;
    }

    public void CancelCodingEvent(long codingEventId) {
      _codingEventRepository.CancelCodingEvent(codingEventId);
    }

    public List<MemberDto> GetMembersList(long codingEventId, ClaimsPrincipal authedUser) {
      var requestingMember =
        _authedUserService.ConvertAuthedUserToMember(codingEventId, authedUser);

      var codingEvent = _dbContext.CodingEvents.Include(ce => ce.Members)
        .ThenInclude(m => m.User)
        .SingleOrDefault(ce => ce.Id == codingEventId);


      return codingEvent?.Members.Select(member => member.ToDto(requestingMember)).ToList();
    }

    public void RemoveMember(long memberId) {
      var memberProxy = new Member() { Id = memberId };
      _dbContext.Members.Remove(memberProxy);
      _dbContext.SaveChanges();
    }
  }
}
