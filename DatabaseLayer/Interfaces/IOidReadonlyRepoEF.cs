using DatabaseLayer.Models.OID;
using System.Linq.Expressions;

namespace DatabaseLayer.Interfaces
{
    public interface IReadonlyAsyncRepoEF<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<UserDashboard>> GetUsersInfoAsync()
        {
            return null;
        }
    }
}
