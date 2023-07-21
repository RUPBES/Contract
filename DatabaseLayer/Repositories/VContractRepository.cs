using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class VContractRepository : IViewRepository<VContract>
    {
        private readonly ContractsContext _context;
        public VContractRepository(ContractsContext context)
        {
            _context = context;
        }

        public int Count()
        {
            return _context.VContracts.Count();
        }

        public IEnumerable<VContract> Find(Func<VContract, bool> predicate)
        {
            return _context.VContracts.Where(predicate).ToList();
        }

        public IEnumerable<VContract> GetAll()
        {
            return _context.VContracts.ToList();
        }

        public VContract GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.VContracts.Find(id);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<VContract> GetEntitySkipTake(int skip, int take)
        {
            return _context.VContracts.OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<VContract> GetEntityWithSkipTake(int skip, int take, int legalPersonId)
        {
            return _context.VContracts/*.Where(x => x.LegalPersonId == legalPersonId)*/.OrderBy(x => x.Date).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<VContract> FindLikeNameObj(string queryString)
        {
            return _context.VContracts.Where(x => EF.Functions.Like(x.NameObject, $"%{queryString}%")).OrderBy(x => x.Date).ToList();
        }
    }
}