namespace DatabaseLayer.Interfaces
{
    public interface IRepositoryShort<T> where T : class
    {
        void Create(T entity);
        T GetById(Guid id, Guid? secondId = null);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Func<T, bool> predicate);
        //void Update(T entity);
        //void Delete(Guid id, int? secondId = null);
    }
}