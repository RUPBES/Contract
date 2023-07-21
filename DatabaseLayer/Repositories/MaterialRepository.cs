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
    internal class MaterialRepository : IRepository<MaterialGc>
    {
        private readonly ContractsContext _context;
        public MaterialRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(MaterialGc entity)
        {
            if (entity is not null)
            {
                _context.MaterialGcs.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            MaterialGc material = _context.MaterialGcs.Find(id);

            if (material is not null)
            {
                _context.MaterialGcs.Remove(material);
            }
        }

        public IEnumerable<MaterialGc> Find(Func<MaterialGc, bool> predicate)
        {
            return _context.MaterialGcs.Where(predicate).ToList();
        }

        public IEnumerable<MaterialGc> GetAll()
        {
            return _context.MaterialGcs.ToList();
        }

        public MaterialGc GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.MaterialGcs.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(MaterialGc entity)
        {
            if (entity is not null)
            {
                var material = _context.MaterialGcs.Find(entity.Id);

                if (material is not null)
                {
                    material.Price = entity.Price;
                    material.FactPrice = entity.FactPrice;
                    material.Period = entity.Period;
                    material.ContractId = entity.ContractId;
                    material.IsChange = entity.IsChange;
                    material.ChangeMaterialId = entity.ChangeMaterialId;


                    _context.MaterialGcs.Update(material);
                }
            }
        }
    }
}
