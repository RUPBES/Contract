using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class SWCostArchiveRepo : IReadonlyRepoEF<SWCost>
    {
        private readonly ContractsArchiveContext _context;
        public SWCostArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<SWCost> Find(Func<SWCost, bool> predicate)
        {
            return _context.SWCosts.Where(predicate).ToList();
        }

        public IEnumerable<SWCost> Find(Func<SWCost, bool> where, Func<SWCost, SWCost> select)
        {
            return _context.SWCosts
                .Where(where).Select(select).ToList();
        }

        public IEnumerable<SWCost> GetAll()
        {
            return _context.SWCosts.ToList();
        }

        public SWCost GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.SWCosts.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}
