using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class PrepaymentFactRepository : IRepository<PrepaymentFact>
    {
        private readonly ContractsContext _context;
        public PrepaymentFactRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(PrepaymentFact entity)
        {
            if (entity is not null)
            {
                _context.PrepaymentFacts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            PrepaymentFact model = _context.PrepaymentFacts.Find(id);

            if (model is not null)
            {
                _context.PrepaymentFacts.Remove(model);
            }
        }

        public IEnumerable<PrepaymentFact> Find(Func<PrepaymentFact, bool> predicate)
        {
            return _context.PrepaymentFacts.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentFact> GetAll()
        {
            return _context.PrepaymentFacts.ToList();
        }

        public PrepaymentFact GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.PrepaymentFacts.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentFact entity)
        {
            if (entity is not null)
            {
                var prepFact = _context.PrepaymentFacts.Find(entity.Id);

                if (prepFact is not null)
                {
                    prepFact.CurrentValue = entity.CurrentValue;
                    prepFact.WorkingOutValue = entity.WorkingOutValue;
                    prepFact.TargetValue = entity.TargetValue;
                    prepFact.Period = entity.Period;
                    prepFact.PrepaymentId= entity.PrepaymentId;
                   
                    _context.PrepaymentFacts.Update(prepFact);
                }
            }
        }
    }
}
