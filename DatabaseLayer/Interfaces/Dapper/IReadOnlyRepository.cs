namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadOnlyRepository<T> where T : class
    {
        int Count(string[]? predicate =null) => 0;
        T GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetPage(int skip, int take, string organizationName) => Array.Empty<T>();

        IEnumerable<T> Find(string predicate);
        IEnumerable<T> Find(string predicate, string[] orgList);
    }
}