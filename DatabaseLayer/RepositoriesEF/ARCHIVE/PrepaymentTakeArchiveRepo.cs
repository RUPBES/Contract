using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class PrepaymentTakeArchiveRepo : IReadonlyRepoEF<PrepaymentTake>
    {
        private readonly ContractsArchiveContext _context;

        public PrepaymentTakeArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<PrepaymentTake> Find(Func<PrepaymentTake, bool> predicate)
        {
            return _context.PrepaymentTakes.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentTake> GetAll()
        {
            return _context.PrepaymentTakes.ToList();
        }

        public PrepaymentTake GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.PrepaymentTakes.Find(id);
            }
            else
            {
                return null;
            }
        }

    }
}

