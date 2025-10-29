using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class CorrespondenceArchiveRepo : IReadonlyRepoEF<Correspondence>
    {
        private readonly ContractsArchiveContext _context;
        public CorrespondenceArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<Correspondence> Find(Func<Correspondence, bool> predicate)
        {
            return _context.Correspondences.Where(predicate).ToList();
        }

        public IEnumerable<Correspondence> GetAll()
        {
            return _context.Correspondences.ToList();
        }

        public Correspondence GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Correspondences.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}
