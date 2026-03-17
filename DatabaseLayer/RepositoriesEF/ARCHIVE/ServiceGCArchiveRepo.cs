using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ServiceGCArchiveRepo : IReadonlyRepoEF<ServiceGc>
    {
        private readonly ContractsArchiveContext _context;
        public ServiceGCArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public IEnumerable<ServiceGc> Find(Func<ServiceGc, bool> predicate)
        {
            return _context.ServiceGcs.Where(predicate).ToList();
        }

        public IEnumerable<ServiceGc> GetAll()
        {
            return _context.ServiceGcs.ToList();
        }

        public ServiceGc GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.ServiceGcs.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

