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
    internal class CommissionActRepository : IRepository<CommissionAct>
    {
        private readonly ContractsContext _context;
        public CommissionActRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(CommissionAct entity)
        {
            if (entity is not null)
            {
                _context.СommissionActs.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            CommissionAct commAct = _context.СommissionActs.Find(id);

            if (commAct is not null)
            {
                _context.СommissionActs.Remove(commAct);
            }
        }

        public IEnumerable<CommissionAct> Find(Func<CommissionAct, bool> predicate)
        {
            return _context.СommissionActs.Where(predicate).ToList();
        }

        public IEnumerable<CommissionAct> GetAll()
        {
            return _context.СommissionActs.ToList();
        }

        public CommissionAct GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.СommissionActs.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(CommissionAct entity)
        {
            if (entity is not null)
            {
                var commAct = _context.СommissionActs.Find(entity.Id);

                if (commAct is not null)
                {
                    commAct.Number = entity.Number;
                    commAct.Date = entity.Date;
                    commAct.ContractId = entity.ContractId;

                    _context.СommissionActs.Update(commAct);
                }
            }
        }
    }
}
