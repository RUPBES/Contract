namespace DatabaseLayer.Interfaces
{
    public interface IReadonlyRepoEF<T> where T : class
    {
        T GetById(int id, int? secondId = null);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Func<T, bool> predicate);
        IEnumerable<T> Find(Func<T, bool> where, Func<T, T> select)
        {
            return new List<T>();
        }        
    }
}
