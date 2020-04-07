using System.Collections.Generic;
using System.Security.Cryptography;
using CodingEventsAPI.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodingEventsAPI.Swagger {
  public class NewTagExample : IExamplesProvider<NewTagDto> {
    public NewTagDto GetExamples() {
      return new NewTagDto {
        Name = "azure",
      };
    }
  }

  public class TagExample : IExamplesProvider<TagDto> {
    public TagDto GetExamples() {
      var mockTag = new Tag {
        Id = RandomNumberGenerator.GetInt32(1, 1000),
        Name = "microsoft"
      };

      return TagDto.ForOwner(mockTag, MemberExample.memberMock);
    }
  }

  public class TagsExample : IExamplesProvider<List<TagDto>> {
    public List<TagDto> GetExamples() {
      var tagExampleGenerator = new TagExample();

      var firstExample = tagExampleGenerator.GetExamples();

      var secondExample = tagExampleGenerator.GetExamples();
      secondExample.Name = "launchcode";

      return new List<TagDto> {
        firstExample,
        secondExample
      };
    }
  }
}
