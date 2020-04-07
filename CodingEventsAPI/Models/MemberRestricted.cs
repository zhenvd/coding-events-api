namespace CodingEventsAPI.Models {
  public interface IMemberRestrictedEntity<out TDto> {
    TDto ToMemberDto(Member requestingMember);
  }
}
