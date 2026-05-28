using DatabaseLayer.Models.KDO;
using System.Linq.Expressions;

namespace DatabaseLayer.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Create(T entity);
        T GetById(int id, int? secondId = null);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Func<T, bool> predicate);
        IEnumerable<T> Find(Func<T, bool> where, Func<T, T> select)
        {
            return new List<T>();
        }
        void Update(T entity);
        void Delete(int id, int? secondId = null);

        ///***
        ///
        Task<IEnumerable<T>> GetAllAsync()
        {
            return null;
        }


        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate)
        {
            return null;
        }
    }
}
