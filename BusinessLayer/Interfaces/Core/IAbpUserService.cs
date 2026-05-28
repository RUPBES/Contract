using DatabaseLayer.Models.OID;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Core
{
    public interface IAbpUserService
    {
        Task<AbpUser> GetById(Guid id);
        void Update(AbpUser item);
        Task<IEnumerable<AbpUser>> Find(Expression<Func<AbpUser, bool>> predicate);
    }
}
