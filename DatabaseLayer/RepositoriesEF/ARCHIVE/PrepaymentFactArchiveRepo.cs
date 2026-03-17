using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PrepaymentFactArchiveRepo : IReadonlyRepoEF<PrepaymentFact>
    {
        private readonly ContractsArchiveContext _context;
        public PrepaymentFactArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<PrepaymentFact> Find(Func<PrepaymentFact, bool> predicate)
        {
            return _context.PrepaymentFacts.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentFact> GetAll()
        {
            return _context.PrepaymentFacts.ToList();
        }

        public PrepaymentFact GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.PrepaymentFacts.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

