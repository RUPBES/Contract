using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class ScopeWorkRepository : IRepository<ScopeWork>
    {
        private readonly ContractsContext _context;
        public ScopeWorkRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ScopeWork entity)
        {
            if (entity is not null)
            {
                _context.ScopeWorks.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            ScopeWork scWork = _context.ScopeWorks.Find(id);

            if (scWork is not null)
            {
                _context.ScopeWorks.Remove(scWork);
            }
        }

        public IEnumerable<ScopeWork> Find(Func<ScopeWork, bool> predicate)
        {
            return _context.ScopeWorks.Where(predicate).ToList();
        }

        public IEnumerable<ScopeWork> GetAll()
        {
            return _context.ScopeWorks.ToList();
        }

        public ScopeWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.ScopeWorks.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(ScopeWork entity)
        {
            if (entity is not null)
            {
                var scWork = _context.ScopeWorks.Find(entity.Id);

                if (scWork is not null)
                {
                    scWork.Period = entity.Period;
                    scWork.CostNoNds = entity.CostNoNds;
                    scWork.CostNds = entity.CostNds;
                    scWork.SmrCost = entity.SmrCost;
                    scWork.PnrCost = entity.PnrCost;

                    scWork.EquipmentCost = entity.EquipmentCost;
                    scWork.OtherExpensesCost = entity.OtherExpensesCost;
                    scWork.AdditionalCost = entity.AdditionalCost;
                    scWork.MaterialCost = entity.MaterialCost;
                    scWork.GenServiceCost = entity.GenServiceCost;
                    scWork.IsChange = entity.IsChange;
                    scWork.IsOwnForces = entity.IsOwnForces;

                    scWork.ContractId = entity.ContractId;
                    scWork.ChangeScopeWorkId = entity.ChangeScopeWorkId;


                    _context.ScopeWorks.Update(scWork);
                }
            }
        }
    }
}
