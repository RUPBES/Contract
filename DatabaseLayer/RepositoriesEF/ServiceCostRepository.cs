using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class ServiceCostRepository : IRepository<ServiceCost>
    {
        private readonly ContractsContext _context;
        public ServiceCostRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ServiceCost entity)
        {
            if (entity is not null)
            {
                _context.ServiceCosts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            ServiceCost? model = _context.ServiceCosts.Find(id);

            if (model is not null)
            {
                _context.ServiceCosts.Remove(model);
            }
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

        public void Update(ServiceCost entity)
        {
            if (entity is not null)
            {
                var service = _context.ServiceCosts.Find(entity.Id);

                if (service is not null)
                {
                    service.Period = entity.Period;
                    service.Price = entity.Price;
                    service.IsFact = entity.IsFact;
                    service.ServiceGCId = entity.ServiceGCId;

                    _context.ServiceCosts.Update(service);
                }
            }
        }
    }
}
