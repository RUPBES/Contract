using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ServiceAmendmentArchiveRepo : IReadonlyRepoEF<ServiceAmendment>
    {
        private readonly ContractsArchiveContext _context;
        public ServiceAmendmentArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<ServiceAmendment> Find(Func<ServiceAmendment, bool> predicate)
        {
            return _context.ServiceAmendments.Where(predicate).ToList();
        }

        public IEnumerable<ServiceAmendment> GetAll()
        {
            return _context.ServiceAmendments.ToList();
        }

        public ServiceAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.ServiceAmendments
                    .FirstOrDefault(x => x.ServiceId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }
    }
}

