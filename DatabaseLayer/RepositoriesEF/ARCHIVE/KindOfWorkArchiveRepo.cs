using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class KindOfWorkArchiveRepo : IReadonlyRepoEF<KindOfWork>
    {
        private readonly ContractsArchiveContext _context;
        public KindOfWorkArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<KindOfWork> Find(Func<KindOfWork, bool> predicate)
        {
            return _context.KindOfWorks.Where(predicate).ToList();
        }

        public IEnumerable<KindOfWork> GetAll()
        {
            return _context.KindOfWorks.ToList();
        }

        public KindOfWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.KindOfWorks.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}
