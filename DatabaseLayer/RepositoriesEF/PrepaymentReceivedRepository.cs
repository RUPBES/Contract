using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.RepositoriesEF
{
    internal class PrepaymentReceivedRepository : IRepository<PrepaymentReceived>
    {
        private readonly ContractsContext _context;
        public PrepaymentReceivedRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(PrepaymentReceived entity)
        {
            if (entity is not null)
            {
                _context.PrepaymentReceiveds.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            PrepaymentReceived model = _context.PrepaymentReceiveds.Find(id);

            if (model is not null)
            {
                _context.PrepaymentReceiveds.Remove(model);
            }
        }

        public IEnumerable<PrepaymentReceived> Find(Func<PrepaymentReceived, bool> predicate)
        {
            return _context.PrepaymentReceiveds.Where(predicate).ToList();
        }

        public IEnumerable<PrepaymentReceived> GetAll()
        {
            return _context.PrepaymentReceiveds.ToList();
        }

        public PrepaymentReceived GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.PrepaymentReceiveds.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentReceived entity)
        {
            if (entity is not null)
            {
                var prepFact = _context.PrepaymentReceiveds.Find(entity.Id);

                if (prepFact is not null)
                {
                    prepFact.CurrentValue = entity.CurrentValue;
                    prepFact.TargetValue = entity.TargetValue;
                    prepFact.Period = entity.Period;
                    prepFact.ContractId = entity.ContractId;

                    _context.PrepaymentReceiveds.Update(prepFact);
                }
                else
                {
                    if (entity is not null)
                    {
                        _context.PrepaymentReceiveds.Add(entity);
                    }
                    }
                }
            }
        }
    }

