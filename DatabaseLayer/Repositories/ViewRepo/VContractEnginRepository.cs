using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ViewRepo
{
    public class VContractEnginRepository : IViewRepository<VContractEngin>
    {
        private readonly ContractsContext _context;
        public VContractEnginRepository(ContractsContext context)
        {
            _context = context;
        }

        public int Count()
        {
            return _context.VContractEngins.Count();
        }

        public IEnumerable<VContractEngin> Find(Func<VContractEngin, bool> predicate)
        {
            return _context.VContractEngins.Where(predicate).ToList();
        }

        public IEnumerable<VContractEngin> GetAll()
        {
            return _context.VContractEngins.ToList();
        }

        public VContractEngin GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.VContractEngins.Find(id);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<VContractEngin> GetEntitySkipTake(int skip, int take, string org)
        {
            return _context.VContractEngins.Where(x => x.Author == org || x.Owner == org).OrderByDescending(x => x.Id).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<VContractEngin> GetEntityWithSkipTake(int skip, int take, int legalPersonId)
        {
            return _context.VContractEngins/*.Where(x => x.LegalPersonId == legalPersonId)*/.OrderBy(x => x.Date).Skip(skip).Take(take).ToList();
        }

        public IEnumerable<VContractEngin> FindLikeNameObj(string queryString)
        {
            return _context.VContractEngins.Where(x => EF.Functions.Like(x.NameObject, $"%{queryString}%")).OrderBy(x => x.Date).ToList();
        }

        public IEnumerable<VContractEngin> FindContract(string queryString)
        {
            return _context.VContractEngins.Where(x => EF.Functions.Like(x.NameObject, $"%{queryString}%") || EF.Functions.Like(x.Number, $"%{queryString}%")).OrderBy(x => x.Date).ToList();
        }
    }
}