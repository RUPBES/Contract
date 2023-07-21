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
    internal class PrepaymentAmendmentRepository : IRepository<PrepaymentAmendment>
    {
        private readonly ContractsContext _context;
        public PrepaymentAmendmentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(PrepaymentAmendment entity)
        {
            if (entity is not null)
            {
                _context.PrepaymentAmendments.Add(entity);
            }
        }

        public void Delete(int id, int? amendId)
        {
            PrepaymentAmendment prepaymentAmendment = null;

            if (id > 0 && amendId != null)
            {
                prepaymentAmendment = _context.PrepaymentAmendments
                    .FirstOrDefault(x => x.PrepaymentId == id && x.AmendmentId == amendId);
            }

            if (prepaymentAmendment is not null)
            {
                _context.PrepaymentAmendments.Remove(prepaymentAmendment);
            }
        }

        public IEnumerable<PrepaymentAmendment> Find(Func<PrepaymentAmendment, bool> predicate)
        {
            return _context.PrepaymentAmendments.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentAmendment> GetAll()
        {
            return _context.PrepaymentAmendments.ToList();
        }

        public PrepaymentAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.PrepaymentAmendments
                    .FirstOrDefault(x => x.PrepaymentId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentAmendment entity)
        {
            if (entity is not null)
            {
                var prepaymentAmendment = _context.PrepaymentAmendments
                    .FirstOrDefault(x => x.PrepaymentId == entity.PrepaymentId && x.AmendmentId == entity.AmendmentId);

                if (prepaymentAmendment is not null)
                {
                    prepaymentAmendment.PrepaymentId = entity.PrepaymentId;
                    prepaymentAmendment.AmendmentId = entity.AmendmentId;

                    _context.PrepaymentAmendments.Update(prepaymentAmendment);
                }
            }
        }
    }
}

