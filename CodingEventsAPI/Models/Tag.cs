using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CodingEventsAPI.Models {
  public class Tag : UniqueEntity {
    public string Name { get; set; }

    [JsonIgnore] // do not serialize these (only accessible in the API code)
    public List<CodingEventTag> CodingEventTags { get; set; }
  }

  public class NewTagDto {
    [NotNull]
    [Required]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Tag must be between 1 and 20 characters")]
    public string Name { get; set; }
  }

  // join table entity for relation between Tag and CodingEvent
  public class CodingEventTag {
    public long TagId { get; set; }
    public Tag Tag { get; set; }

    public long CodingEventId { get; set; }
    public CodingEvent CodingEvent { get; set; }
  }
}
