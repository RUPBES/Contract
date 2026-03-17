using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class EmployeeContractArchiveRepo : IReadonlyRepoEF<EmployeeContract>
    {
        private readonly ContractsArchiveContext _context;
        public EmployeeContractArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<EmployeeContract> Find(Func<EmployeeContract, bool> predicate)
        {
            return _context.EmployeeContracts.Where(predicate).ToList();
        }

        public IEnumerable<EmployeeContract> GetAll()
        {
            return _context.EmployeeContracts.ToList();
        }

        public EmployeeContract GetById(int id, int? contractId = null)
        {
            if (id > 0 && contractId != null)
            {
                return _context.EmployeeContracts
                    .FirstOrDefault(x => x.EmployeeId == id && x.ContractId == contractId);
            }
            else
            {
                return null;
            }
        }
    }
}
