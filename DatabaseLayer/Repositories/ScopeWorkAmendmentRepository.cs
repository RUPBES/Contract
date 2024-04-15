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
    internal class ScopeWorkAmendmentRepository : IRepository<ScopeWorkAmendment>
    {
        private readonly ContractsContext _context;
        public ScopeWorkAmendmentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ScopeWorkAmendment entity)
        {
            if (entity is not null)
            {
                _context.ScopeWorkAmendments.Add(entity);
            }
        }

        public void Delete(int id, int? amendId)
        {
            ScopeWorkAmendment scopeWorkAmendment = null;

            if (id > 0 && amendId != null)
            {
                scopeWorkAmendment = _context.ScopeWorkAmendments
                    .FirstOrDefault(x => x.ScopeWorkId == id && x.AmendmentId == amendId);
            }

            if (scopeWorkAmendment is not null)
            {
                _context.ScopeWorkAmendments.Remove(scopeWorkAmendment);
            }
        }

        public IEnumerable<ScopeWorkAmendment> Find(Func<ScopeWorkAmendment, bool> predicate)
        {
            return _context.ScopeWorkAmendments.Where(predicate).ToList();
        }

        public IEnumerable<ScopeWorkAmendment> GetAll()
        {
            return _context.ScopeWorkAmendments.ToList();
        }

        public ScopeWorkAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.ScopeWorkAmendments
                    .FirstOrDefault(x => x.ScopeWorkId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }

        public void Update(ScopeWorkAmendment entity)
        {
            if (entity is not null)
            {
                var materialAmend = _context.ScopeWorkAmendments
                    .FirstOrDefault(x => x.ScopeWorkId == entity.ScopeWorkId && x.AmendmentId == entity.AmendmentId);

                if (materialAmend is not null)
                {
                    materialAmend.ScopeWorkId = entity.ScopeWorkId;
                    materialAmend.AmendmentId = entity.AmendmentId;

                    _context.ScopeWorkAmendments.Update(materialAmend);
                }
            }
        }
    }
}
