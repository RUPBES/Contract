using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Interfaces
{
    public interface IUserDailyVisitRepository
    {

        //Task<UserDailyVisit?> GetDetailAsync(int id, string userId);


        // ── Административные методы ───────────────────────────────────────────

        //Task<IEnumerable<UserDailyVisit>> GetAllAsync();

        //Task<UserDailyVisit?> GetByIdAsync(int id);

        void Create(UserDailyVisit dto);

        Task<int> UpdateLastVisitAsync(Expression<Func<UserDailyVisit, bool>> predicate, DateTime lastVisitUtc);

        Task<IEnumerable<UserDailyVisit>> FindAsync(Expression<Func<UserDailyVisit, bool>> predicate);

        //Task<bool> DeleteAsync(int id);
    }
}