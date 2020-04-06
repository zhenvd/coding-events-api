using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CodingEventsAPI.Models {
  public class CodingEvent : UniqueEntity {
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    [JsonIgnore] // does not get serialized (only accessible within the API code)
    public List<CodingEventTag> CodingEventTags { get; set; } = new List<CodingEventTag>();
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
}
