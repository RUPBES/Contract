using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ServiceCostArchiveRepo : IReadonlyRepoEF<ServiceCost>
    {
        private readonly ContractsArchiveContext _context;
        public ServiceCostArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<ServiceCost> Find(Func<ServiceCost, bool> predicate)
        {
            return _context.ServiceCosts.Where(predicate).ToList();
        }

        public IEnumerable<ServiceCost> GetAll()
        {
            return _context.ServiceCosts.ToList();
        }

        public ServiceCost GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.ServiceCosts.Find(id);
            }
            else
            {
                return null;
            }
        }
    }
}

