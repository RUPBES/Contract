using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ScopeWorkAmendmentArchiveRepo : IReadonlyRepoEF<ScopeWorkAmendment>
    {
        private readonly ContractsArchiveContext _context;
        public ScopeWorkAmendmentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<ScopeWorkAmendment> Find(Func<ScopeWorkAmendment, bool> predicate)
        {
            return _context.ScopeWorkAmendments.Where(predicate).ToList();
        }

        public IEnumerable<ScopeWorkAmendment> GetAll()
        {
            return _context.ScopeWorkAmendments.ToList();
        }

        public ScopeWorkAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.ScopeWorkAmendments
                    .FirstOrDefault(x => x.ScopeWorkId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }
    }
}

