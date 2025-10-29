using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class MaterialCostRepository : IRepository<MaterialCost>
    {
        private readonly ContractsContext _context;
        public MaterialCostRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(MaterialCost entity)
        {
            if (entity is not null)
            {
                _context.MaterialCosts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            MaterialCost model = _context.MaterialCosts.Find(id);

            if (model is not null)
            {
                _context.MaterialCosts.Remove(model);
            }
        }

        public IEnumerable<MaterialCost> Find(Func<MaterialCost, bool> predicate)
        {
            return _context.MaterialCosts.Where(predicate).ToList();
        }

        public IEnumerable<MaterialCost> GetAll()
        {
            return _context.MaterialCosts.ToList();
        }

        public MaterialCost GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.MaterialCosts.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(MaterialCost entity)
        {
            if (entity is not null)
            {
                var material = _context.MaterialCosts.Find(entity.Id);

                if (material is not null)
                {
                    material.Period = entity.Period;                    
                    material.Price = entity.Price;
                    material.IsFact = entity.IsFact;
                    material.MaterialId = entity.MaterialId;

                    _context.MaterialCosts.Update(material);
                }
            }
        }
    }
}
