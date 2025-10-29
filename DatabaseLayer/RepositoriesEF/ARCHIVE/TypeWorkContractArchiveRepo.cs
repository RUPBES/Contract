using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class TypeWorkContractArchiveRepo : IReadonlyRepoEF<TypeWorkContract>
    {
        private readonly ContractsArchiveContext _context;
        public TypeWorkContractArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<TypeWorkContract> Find(Func<TypeWorkContract, bool> predicate)
        {
            return _context.TypeWorkContracts.Include(x => x.TypeWork).Where(predicate).ToList();
        }

        public IEnumerable<TypeWorkContract> GetAll()
        {
            return _context.TypeWorkContracts.ToList();
        }

        public TypeWorkContract GetById(int id, int? contractId = null)
        {
            if (id > 0 && contractId != null)
            {
                return _context.TypeWorkContracts
                    .FirstOrDefault(x => x.TypeWorkId == id && x.ContractId == contractId);
            }
            else
            {
                return null;
            }
        }
    }
}