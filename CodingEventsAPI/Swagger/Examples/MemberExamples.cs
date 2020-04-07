using System.Collections.Generic;
using System.Security.Cryptography;
using CodingEventsAPI.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodingEventsAPI.Swagger {
  public class MemberExample : IExamplesProvider<MemberDto> {
    public static Member memberMock = new Member {
      Id = RandomNumberGenerator.GetInt32(1, 1000),
      CodingEvent = CodingEventExample.mockCodingEvent,
      Role = MemberRole.Member,
      User = new User {
        Id = RandomNumberGenerator.GetInt32(1, 1000),
        Email = "patrick@launchcode.org",
        Username = "the-vampiire",
      },
    };

    public MemberDto GetExamples() {
      return MemberDto.ForMember(memberMock);
    }
  }

  public class MembersExample : IExamplesProvider<List<MemberDto>> {
    public List<MemberDto> GetExamples() {
      var exampleGenerator = new MemberExample();

      var firstExample = exampleGenerator.GetExamples();

      var secondExample = exampleGenerator.GetExamples();
      secondExample.Username = "pdmxdd";
      secondExample.Role = MemberRole.Owner.ToString();

      return new List<MemberDto> {
        firstExample,
        secondExample,
      };
    }
  }
}
