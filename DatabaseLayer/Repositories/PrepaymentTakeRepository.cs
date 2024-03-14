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
    internal class PrepaymentTakeRepository : IRepository<PrepaymentTake>
    {
        private readonly ContractsContext _context;

        public PrepaymentTakeRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(PrepaymentTake entity)
        {
            if (entity is not null)
            {
                _context.PrepaymentTakes.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            PrepaymentTake model = _context.PrepaymentTakes.Find(id);

            if (model is not null)
            {
                _context.PrepaymentTakes.Remove(model);
            }
        }

        public IEnumerable<PrepaymentTake> Find(Func<PrepaymentTake, bool> predicate)
        {
            return _context.PrepaymentTakes.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentTake> GetAll()
        {
            return _context.PrepaymentTakes.ToList();
        }

        public PrepaymentTake GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.PrepaymentTakes.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentTake entity)
        {
            if (entity is not null)
            {
                var prepTake = _context.PrepaymentTakes.Find(entity.Id);

                if (prepTake is not null)
                {
                    prepTake.IsRefund = entity.IsRefund;
                    prepTake.IsTarget = entity.IsTarget;
                    prepTake.Total = entity.Total;
                    prepTake.DateTransfer = entity.DateTransfer;
                    prepTake.PrepaymentId = entity.PrepaymentId;
                    prepTake.FileId = entity.FileId;

                    _context.PrepaymentTakes.Update(prepTake);
                }
            }
        }
    }
}
