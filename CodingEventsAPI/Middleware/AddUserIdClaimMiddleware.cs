using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CodingEventsAPI.Data;
using CodingEventsAPI.Models;
using CodingEventsAPI.Services;
using Microsoft.AspNetCore.Http;

namespace CodingEventsAPI.Middleware {
  public class AddUserIdClaimMiddleware {
    private readonly RequestDelegate _next;

    public AddUserIdClaimMiddleware(RequestDelegate next) {
      _next = next;
    }

    public Task InvokeAsync(HttpContext context, IAuthedUserService authedUserService) {
      var authedUser = context.User;

      // not authenticated or already has the userId claim added (unlikely but guard for future)
      if (!authedUser.Identity.IsAuthenticated || authedUser.FindFirstValue("userId") != null) {
        return _next(context);
      }

      var user = authedUserService.GetOrCreateUserFromActiveDirectory(authedUser);

      // inject user id into context.User
      authedUser.Identities.FirstOrDefault()
        ? // prevent NPE if no identity is found (unlikely but suppress warning)
        .AddClaim(new Claim("userId", user.Id.ToString()));

      return _next(context);
    }
  }
}
