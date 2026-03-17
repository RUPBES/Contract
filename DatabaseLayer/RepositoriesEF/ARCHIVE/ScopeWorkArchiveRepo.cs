using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ScopeWorkArchiveRepo : IReadonlyRepoEF<ScopeWork>
    {
        private readonly ContractsArchiveContext _context;
        public ScopeWorkArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<ScopeWork> Find(Func<ScopeWork, bool> predicate)
        {
            return _context.ScopeWorks
                .Include(x => x.SWCosts)
                .Where(predicate)
                .ToList();
        }

        public IEnumerable<ScopeWork> GetAll()
        {
            return _context.ScopeWorks
                .Include(x => x.SWCosts)
                .ToList();
        }

        public ScopeWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.ScopeWorks.Include(x => x.SWCosts).FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

    }
}

