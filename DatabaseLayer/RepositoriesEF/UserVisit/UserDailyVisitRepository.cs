using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.EXTRA;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DatabaseLayer.RepositoriesEF.UserVisit
{
    internal class UserDailyVisitRepository : IUserDailyVisitRepository
    {

        private readonly ContractsContext _context;
        public UserDailyVisitRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(UserDailyVisit dto)
        {
            _context.UserDailyVisits.Add(new UserDailyVisit()
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                Date = dto.Date,
                FirstVisitUtc = dto.FirstVisitUtc,
                LastVisitUtc = dto.LastVisitUtc,
            });
            _context.SaveChanges();
        }

        public async Task<IEnumerable<UserDailyVisit>> FindAsync(Expression<Func<UserDailyVisit, bool>> predicate)
        {
            return await _context.UserDailyVisits.AsNoTracking()
                .Where(predicate).ToListAsync();
        }


        public async Task<int> UpdateLastVisitAsync(Expression<Func<UserDailyVisit, bool>> predicate, DateTime lastVisitUtc)
        {
            return await _context.UserDailyVisits.Where(predicate).ExecuteUpdateAsync(s => s.SetProperty(v => v.LastVisitUtc, lastVisitUtc));
        }
    }
}