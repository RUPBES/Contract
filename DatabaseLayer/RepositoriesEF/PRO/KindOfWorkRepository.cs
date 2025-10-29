using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Repositories.PRO
{
    internal class KindOfWorkRepository : IRepository<KindOfWork>
    {
        private readonly ContractsContext _context;
        public KindOfWorkRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(KindOfWork entity)
        {
            if (entity is not null)
            {
                _context.KindOfWorks.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            KindOfWork kindOfWorks = _context.KindOfWorks.Find(id);

            if (kindOfWorks is not null)
            {
                _context.KindOfWorks.Remove(kindOfWorks);
            }
        }

        public IEnumerable<KindOfWork> Find(Func<KindOfWork, bool> predicate)
        {
            return _context.KindOfWorks.Where(predicate).ToList();
        }

        public IEnumerable<KindOfWork> GetAll()
        {
            return _context.KindOfWorks.ToList();
        }

        public KindOfWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.KindOfWorks.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(KindOfWork entity)
        {
            if (entity is not null)
            {
                var kindOfWorks = _context.KindOfWorks.Find(entity.Id);

                if (kindOfWorks is not null)
                { 
                    kindOfWorks.name = entity.name;                
                    _context.KindOfWorks.Update(kindOfWorks);
                }
            }
        }
    }
}
