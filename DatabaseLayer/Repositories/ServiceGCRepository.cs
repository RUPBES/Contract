using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class ServiceGCRepository : IRepository<ServiceGc>
    {
        private readonly ContractsContext _context;
        public ServiceGCRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ServiceGc entity)
        {
            if (entity is not null)
            {
                _context.ServiceGcs.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            ServiceGc service = _context.ServiceGcs.Find(id);

            if (service is not null)
            {
                _context.ServiceGcs.Remove(service);
            }
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

        public void Update(ServiceGc entity)
        {
            if (entity is not null)
            {
                var service = _context.ServiceGcs.Find(entity.Id);

                if (service is not null)
                {
                    service.ServicePercent = entity.ServicePercent;
                    service.Period = entity.Period;
                    service.Price = entity.Price;
                    service.FactPrice = entity.FactPrice;
                    service.IsChange = entity.IsChange;
                    service.ContractId = entity.ContractId;
                    service.ChangeServiceId = entity.ChangeServiceId;

                    _context.ServiceGcs.Update(service);
                }
            }
        }
    }
}
