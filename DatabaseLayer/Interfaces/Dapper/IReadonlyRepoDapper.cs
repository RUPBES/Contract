using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyRepoDapper<T> where T : class
    {
        int Count(string[]? predicate =null, string? databaseName = null) => 0;
        T GetById(int id, string? databaseName = null);
        IEnumerable<T> GetAll( string? databaseName = null);
        IEnumerable<T> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName = null) => Array.Empty<T>();
        IEnumerable<T> Find(string predicate, string[] orgList, string? databaseName = null);

        (IEnumerable<T>, int) Filter(int skip, int take, string? query, string? orderBy, string? databaseName = null) => (Array.Empty<T>(), 0);
        (IEnumerable<T>, int) Filter(int skip, int take, string org, string? query, string? orderBy, string? databaseName = null) => (Array.Empty<T>(), 0);
       
    }
}