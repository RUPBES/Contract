using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class TypeWorkRepository : IRepository<TypeWork>
    {
        private readonly ContractsContext _context;
        public TypeWorkRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(TypeWork entity)
        {
            if (entity is not null)
            {
                _context.TypeWorks.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            TypeWork typeWork = null;

            if (id > 0)
            {
                typeWork = _context.TypeWorks.Find(id);
            }

            if (typeWork is not null)
            {
                _context.TypeWorks.Remove(typeWork);
            }
        }

        public IEnumerable<TypeWork> Find(Func<TypeWork, bool> predicate)
        {
            return _context.TypeWorks.Include(x => x.TypeWorkContracts).Where(predicate).ToList();
        }

        public IEnumerable<TypeWork> GetAll()
        {
            return _context.TypeWorks.Include(x=>x.TypeWorkContracts).ToList();
        }

        public TypeWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.TypeWorks.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(TypeWork entity)
        {
            if (entity is not null)
            {
                var typeWork = _context.TypeWorks.Find(entity.Id);

                if (typeWork is not null)
                {
                    typeWork.Name = entity.Name;

                    _context.TypeWorks.Update(typeWork);
                }
            }
        }
    }
}