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
    internal class SWCostRepository : IRepository<SWCost>
    {
        private readonly ContractsContext _context;
        public SWCostRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(SWCost entity)
        {
            if (entity is not null)
            {
                _context.SWCosts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            SWCost model = _context.SWCosts.Find(id);

            if (model is not null)
            {
                _context.SWCosts.Remove(model);
            }
        }

        public IEnumerable<SWCost> Find(Func<SWCost, bool> predicate)
        {
            return _context.SWCosts.Where(predicate).ToList();
        }

        public IEnumerable<SWCost> GetAll()
        {
            return _context.SWCosts.ToList();
        }

        public SWCost GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.SWCosts.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(SWCost entity)
        {
            if (entity is not null)
            {
                var scWork = _context.SWCosts.Find(entity.Id);

                if (scWork is not null)
                {
                    scWork.Period = entity.Period;
                    scWork.CostNoNds = entity.CostNoNds;
                    scWork.CostNds = entity.CostNds;
                    scWork.SmrCost = entity.SmrCost?? 0;
                    scWork.PnrCost = entity.PnrCost?? 0;

                    scWork.EquipmentCost = entity.EquipmentCost ?? 0;
                    scWork.OtherExpensesCost = entity.OtherExpensesCost ?? 0 ;
                    scWork.AdditionalCost = entity.AdditionalCost ?? 0 ;
                    scWork.MaterialCost = entity.MaterialCost ?? 0 ;
                    scWork.GenServiceCost = entity.GenServiceCost ?? 0 ;
                    scWork.ScopeWorkId = entity.ScopeWorkId;
                    scWork.IsOwnForces = entity.IsOwnForces;

                    _context.SWCosts.Update(scWork);
                }
            }
        }
    }
}
