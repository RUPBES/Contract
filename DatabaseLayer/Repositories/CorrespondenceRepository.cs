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
    internal class CorrespondenceRepository : IRepository<Correspondence>
    {
        private readonly ContractsContext _context;
        public CorrespondenceRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(Correspondence entity)
        {
            if (entity is not null)
            {
                _context.Correspondences.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            Correspondence corresp = _context.Correspondences.Find(id);

            if (corresp is not null)
            {
                _context.Correspondences.Remove(corresp);
            }
        }

        public IEnumerable<Correspondence> Find(Func<Correspondence, bool> predicate)
        {
            return _context.Correspondences.Where(predicate).ToList();
        }

        public IEnumerable<Correspondence> GetAll()
        {
            return _context.Correspondences.ToList();
        }

        public Correspondence GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Correspondences.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Correspondence entity)
        {
            if (entity is not null)
            {
                var commAct = _context.Correspondences.Find(entity.Id);

                if (commAct is not null)
                {
                    commAct.Number = entity.Number;
                    commAct.Date = entity.Date;
                    commAct.Summary = entity.Summary;
                    commAct.IsInBox = entity.IsInBox;
                    commAct.ContractId = entity.ContractId;

                    _context.Correspondences.Update(commAct);
                }
            }
        }
    }
}
