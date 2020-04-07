using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CodingEventsAPI.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodingEventsAPI.Swagger {
  public class NewCodingEventExample : IExamplesProvider<NewCodingEventDto> {
    public NewCodingEventDto GetExamples() {
      return new NewCodingEventDto {
        Title = "New Code Event title",
        Description = "New Code Event description",
        Date = DateTime.Now,
      };
    }
  }


  public class CodingEventExample : IExamplesProvider<CodingEventDto> {
    public static CodingEvent mockCodingEvent = new CodingEvent {
      Id = RandomNumberGenerator.GetInt32(1, 1000),
      Date = DateTime.Today,
      Title = "LaunchCode: Introduction to Azure",
      Description = "A one week course on deploying a RESTful API to Azure",
    };

    public CodingEventDto GetExamples() {
      return CodingEventDto.ForOwner(mockCodingEvent);
    }
  }

  public class CodingEventsExample : IExamplesProvider<List<CodingEventDto>> {
    public List<CodingEventDto> GetExamples() {
      var exampleGenerator = new CodingEventExample();

      var firstExample = exampleGenerator.GetExamples();

      var secondExample = exampleGenerator.GetExamples();
      secondExample.Title = "LaunchCode: Introduction to DS & A";
      secondExample.Description = "A one week course on Data Structures & Algorithms";

      return new List<CodingEventDto> {
        firstExample,
        secondExample,
      };
    }
  }
}
