using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Repositories.PRO
{
    internal class AbbreviationKindOfWorkRepository : IRepository<AbbreviationKindOfWork>
    {
        private readonly ContractsContext _context;
        public AbbreviationKindOfWorkRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(AbbreviationKindOfWork entity)
        {
            if (entity is not null)
            {
                _context.AbbreviationKindOfWorks.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            AbbreviationKindOfWork abbreviationKindOfWork = _context.AbbreviationKindOfWorks.Find(id);

            if (abbreviationKindOfWork is not null)
            {
                _context.AbbreviationKindOfWorks.Remove(abbreviationKindOfWork);
            }
        }

        public IEnumerable<AbbreviationKindOfWork> Find(Func<AbbreviationKindOfWork, bool> predicate)
        {
            return _context.AbbreviationKindOfWorks.Where(predicate).ToList();
        }

        public IEnumerable<AbbreviationKindOfWork> GetAll()
        {
            return _context.AbbreviationKindOfWorks.ToList();
        }

        public AbbreviationKindOfWork GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.AbbreviationKindOfWorks.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(AbbreviationKindOfWork entity)
        {
            if (entity is not null)
            {
                var abbreviationKindOfWorks = _context.AbbreviationKindOfWorks.Find(entity.Id);

                if (abbreviationKindOfWorks is not null)
                {
                    abbreviationKindOfWorks.name = entity.name;
                    abbreviationKindOfWorks.KindOfWorkId = entity.KindOfWorkId;  
                    _context.AbbreviationKindOfWorks.Update(abbreviationKindOfWorks);
                }
            }
        }
    }
}
