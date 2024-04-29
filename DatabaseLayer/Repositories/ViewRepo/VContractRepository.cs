using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DatabaseLayer.Repositories.ViewRepo
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

        public IEnumerable<VContract> GetEntitySkipTake(int skip, int take, string org)
        {   
            var list = org.Split(',');
            return _context.VContracts
                .Where(x => /*list.Contains(x.Author) ||*/ list.Contains(x.Owner))
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public IEnumerable<VContract> GetEntityWithSkipTake(int skip, int take, int legalPersonId)
        {
            return _context.VContracts/*.Where(x => x.LegalPersonId == legalPersonId)*/.OrderBy(x => x.Date).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<VContract> FindLikeNameObj(string queryString)
        {
            return _context.VContracts.Where(x => EF.Functions.Like(x.NameObject, $"%{queryString}%")).OrderBy(x => x.Date).ToList();
        }

        public IEnumerable<VContract> FindContract(string queryString)
        {
            return _context.VContracts.Where(x => EF.Functions.Like(x.NameObject, $"%{queryString}%") || EF.Functions.Like(x.Number, $"%{queryString}%")).OrderBy(x => x.Date).ToList();
        }
    }
}