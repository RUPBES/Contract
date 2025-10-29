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
    internal class ActRepository : IRepository<Act>
    {
        private readonly ContractsContext _context;
        public ActRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Act entity)
        {
            if (entity is not null)
            {
                _context.Acts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Act act = _context.Acts.Find(id);

            if (act is not null)
            {
                _context.Acts.Remove(act);
            }
        }

        public IEnumerable<Act> Find(Func<Act, bool> predicate)
        {
            return _context.Acts.Where(predicate).ToList();
        }

        public IEnumerable<Act> GetAll()
        {
            return _context.Acts.ToList();
        }

        public Act GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Acts.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Act entity)
        {
            if (entity is not null)
            {
                var act = _context.Acts.Find(entity.Id);

                if (act is not null)
                {
                    act.Reason = entity.Reason;
                    act.DateRenewal = entity.DateRenewal;
                    act.DateSuspendedFrom = entity.DateSuspendedFrom;
                    act.DateSuspendedUntil = entity.DateSuspendedUntil;
                    act.IsSuspension = entity.IsSuspension;
                    act.ContractId = entity.ContractId;
                    act.DateAct = entity.DateAct;


                    _context.Acts.Update(act);
                }
            }
        }
    }
}
