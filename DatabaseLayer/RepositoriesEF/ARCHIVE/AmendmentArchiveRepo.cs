using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class AmendmentArchiveRepo : IReadonlyRepoEF<Amendment>
    {
        private readonly ContractsArchiveContext _context;
        public AmendmentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<Amendment> Find(Func<Amendment, bool> predicate)
        {
            return _context.Amendments.Where(predicate).ToList();
        }

        public IEnumerable<Amendment> GetAll()
        {
            return _context.Amendments.ToList();
        }

        public Amendment GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Amendments.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}


