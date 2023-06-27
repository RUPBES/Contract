namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IService<T, K> where T : class where K : class
    {
        int? Create(T item);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Func<K, bool> predicate);
        T GetById(int id, int? secondId = null);
        void Update(T item);
        void Delete(int id, int? secondId = null);
    }
}