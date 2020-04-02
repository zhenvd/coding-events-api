using System;

namespace CodingEventsAPI.Models {
  public class CodingEvent {
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
  }
}
