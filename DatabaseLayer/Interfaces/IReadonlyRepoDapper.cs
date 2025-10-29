using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces
{
    public interface IReadonlyRepoDapper<T> where T : class
    {
        int Count() => 0;
        T GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetEntitySkipTake(int skip, int take, string organizationName) => Array.Empty<T>();

        IEnumerable<T> Find(string predicate);
        IEnumerable<T> Find(string predicate, string[] orgList);
    }
}