using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class MaterialAmendmentArchiveRepo : IReadonlyRepoEF<MaterialAmendment>
    {
        private readonly ContractsArchiveContext _context;
        public MaterialAmendmentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<MaterialAmendment> Find(Func<MaterialAmendment, bool> predicate)
        {
            return _context.MaterialAmendments.Where(predicate).ToList();
        }

        public IEnumerable<MaterialAmendment> GetAll()
        {
            return _context.MaterialAmendments.ToList();
        }

        public MaterialAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.MaterialAmendments
                    .FirstOrDefault(x => x.MaterialId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }
    }
}

