using System.Collections.Generic;
using System.Security.Claims;

namespace CodingEventsAPI.Models {
  public class User : UniqueEntity {
    public User() { }

    public User(ClaimsPrincipal authedUser) {
      Username = authedUser.FindFirstValue("name");
      Email = authedUser.FindFirstValue("emails");
      AzureOId = authedUser.FindFirstValue(
        "http://schemas.microsoft.com/identity/claims/objectidentifier"
      );
    }

    // azure provider unique ID
    public string AzureOId { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public List<Member> Memberships { get; set; }
  }
}
