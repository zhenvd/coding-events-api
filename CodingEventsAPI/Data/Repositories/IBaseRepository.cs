namespace CodingEventsAPI.Data.Repositories {
  public interface IBaseRepository<TEntity> {
    bool Exists(long entityId);
  }
}
