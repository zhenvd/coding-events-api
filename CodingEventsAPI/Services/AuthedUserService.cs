using System;
using System.Linq;
using System.Security.Claims;
using CodingEventsAPI.Data;
using CodingEventsAPI.Models;

namespace CodingEventsAPI.Services {
  public interface IAuthedUserService {
    User GetOrCreateUserFromActiveDirectory(ClaimsPrincipal activeDirectoryUser);

    User ConvertAuthedUserToUser(ClaimsPrincipal authedUser);

    Member ConvertAuthedUserToMember(long codeEventId, ClaimsPrincipal authedUser);

    CodingEvent RegisterCodingEvent(
      NewCodingEventDto newCodingEventDto,
      ClaimsPrincipal authedUser
    );

    void JoinCodingEvent(long codingEventId, ClaimsPrincipal authedUser);
  }

  public class AuthedUserService : IAuthedUserService {
    private readonly CodingEventsDbContext _dbContext;

    public AuthedUserService(CodingEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    public User GetOrCreateUserFromActiveDirectory(ClaimsPrincipal activeDirectoryUser) {
      var newUser = new User(activeDirectoryUser);

      var existingUser = _dbContext.Users.FirstOrDefault(u => u.AzureOId == newUser.AzureOId);
      if (existingUser != null) return existingUser;

      _dbContext.Users.Add(newUser);
      _dbContext.SaveChanges();
      return newUser;
    }

    public User ConvertAuthedUserToUser(ClaimsPrincipal authedUser) {
      var authedUserId = Convert.ToInt64(authedUser.FindFirstValue("userId"));
      return _dbContext.Users.Find(authedUserId);
    }

    public Member ConvertAuthedUserToMember(long codeEventId, ClaimsPrincipal authedUser) {
      var user = ConvertAuthedUserToUser(authedUser);

      return _dbContext.Members.First(m => m.UserId == user.Id && m.CodingEventId == codeEventId);
    }

    public CodingEvent RegisterCodingEvent(
      NewCodingEventDto newCodingEvent,
      ClaimsPrincipal authedUser
    ) {
      var user = ConvertAuthedUserToUser(authedUser);

      var codingEventEntry = _dbContext.CodingEvents.Add(new CodingEvent());
      codingEventEntry.CurrentValues.SetValues(newCodingEvent);
      var codingEvent = codingEventEntry.Entity;

      _dbContext.Members.Add(Member.CreateEventOwner(codingEvent, user));

      _dbContext.SaveChanges();

      return codingEvent;
    }

    public void JoinCodingEvent(long codingEventId, ClaimsPrincipal authedUser) {
      var user = ConvertAuthedUserToUser(authedUser);
      _dbContext.Members.Add(Member.CreateEventMember(codingEventId, user.Id));
      _dbContext.SaveChanges();
    }
  }
}
