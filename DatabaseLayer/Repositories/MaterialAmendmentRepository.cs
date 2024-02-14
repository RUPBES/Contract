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
    internal class MaterialAmendmentRepository : IRepository<MaterialAmendment>
    {
        private readonly ContractsContext _context;
        public MaterialAmendmentRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(MaterialAmendment entity)
        {
            if (entity is not null)
            {
                _context.MaterialAmendments.Add(entity);
            }
        }

        public void Delete(int id, int? amendId)
        {
            MaterialAmendment materAmendment = null;

            if (id > 0 && amendId != null)
            {
                materAmendment = _context.MaterialAmendments
                    .FirstOrDefault(x => x.MaterialId == id && x.AmendmentId == amendId);
            }

            if (materAmendment is not null)
            {
                _context.MaterialAmendments.Remove(materAmendment);
            }
        }

        public IEnumerable<MaterialAmendment> Find(Func<MaterialAmendment, bool> predicate)
        {
            return _context.MaterialAmendments.Where(predicate).ToList();
        }

        public IEnumerable<MaterialAmendment> GetAll()
        {
            return _context.MaterialAmendments.ToList();
        }

        public MaterialAmendment GetById(int id, int? amendId)
        {
            if (id > 0 && amendId != null)
            {
                return _context.MaterialAmendments
                    .FirstOrDefault(x => x.MaterialId == id && x.AmendmentId == amendId);
            }
            else
            {
                return null;
            }
        }

        public void Update(MaterialAmendment entity)
        {
            if (entity is not null)
            {
                var materialAmend = _context.MaterialAmendments
                    .FirstOrDefault(x => x.MaterialId == entity.MaterialId && x.AmendmentId == entity.AmendmentId);

                if (materialAmend is not null)
                {
                    materialAmend.MaterialId = entity.MaterialId;
                    materialAmend.AmendmentId = entity.AmendmentId;

                    _context.MaterialAmendments.Update(materialAmend);
                }
            }
        }
    }
}
