
namespace DatabaseLayer.Interfaces
{
    public interface IEntityWithPagingRepository<T> : IRepository<T> where T : class
    {
        int Count();
        IEnumerable<T> GetEntitySkipTake(int skip, int take);
        IEnumerable<T> GetEntityWithSkipTake(int skip, int take, string org );
        IEnumerable<T> FindLike(string propName, string queryString);
    }
}