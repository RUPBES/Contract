using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class ServiceAmendmentRepository : IRepository<ServiceAmendment>
    {
        private readonly ContractsContext _context;
        public ServiceAmendmentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ServiceAmendment entity)
        {
            if (entity is not null)
            {
                _context.ServiceAmendments.Add(entity);
            }
        }

        public void Delete(int id, int? amendId)
        {
            ServiceAmendment serviceAmendment = null;

            if (id > 0 && amendId != null)
            {
                serviceAmendment = _context.ServiceAmendments
                    .FirstOrDefault(x => x.ServiceId == id && x.AmendmentId == amendId);
            }

            if (serviceAmendment is not null)
            {
                _context.ServiceAmendments.Remove(serviceAmendment);
            }
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

        public void Update(ServiceAmendment entity)
        {
            if (entity is not null)
            {
                var serviceAmendment = _context.ServiceAmendments
                    .FirstOrDefault(x => x.ServiceId == entity.ServiceId && x.AmendmentId == entity.AmendmentId);

                if (serviceAmendment is not null)
                {
                    serviceAmendment.ServiceId = entity.ServiceId;
                    serviceAmendment.AmendmentId = entity.AmendmentId;

                    _context.ServiceAmendments.Update(serviceAmendment);
                }
            }
        }
    }
}
