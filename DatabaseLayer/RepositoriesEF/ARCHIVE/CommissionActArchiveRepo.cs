using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class CommissionActArchiveRepo : IReadonlyRepoEF<CommissionAct>
    {
        private readonly ContractsArchiveContext _context;
        public CommissionActArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<CommissionAct> Find(Func<CommissionAct, bool> predicate)
        {
            return _context.СommissionActs.Where(predicate).ToList();
        }

        public IEnumerable<CommissionAct> GetAll()
        {
            return _context.СommissionActs.ToList();
        }

        public CommissionAct GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.СommissionActs.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}
