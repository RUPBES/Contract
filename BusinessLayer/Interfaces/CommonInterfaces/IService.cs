namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IService<T, K> where T : class where K : class
    {
        void Create(T item);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Func<K, bool> predicate);
        T GetById(int id);
        void Update(T item);
        void Delete(int id);
    }
}