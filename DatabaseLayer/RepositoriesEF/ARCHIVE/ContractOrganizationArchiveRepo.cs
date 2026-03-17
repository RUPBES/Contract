using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ContractOrganizationArchiveRepo : IReadonlyRepoEF<ContractOrganization>
    {
        private readonly ContractsArchiveContext _context;
        public ContractOrganizationArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }
        public IEnumerable<ContractOrganization> Find(Func<ContractOrganization, bool> predicate)
        {
            return _context.ContractOrganizations.Where(predicate).ToList();
        }

        public IEnumerable<ContractOrganization> GetAll()
        {
            return _context.ContractOrganizations.ToList();
        }

        public ContractOrganization GetById(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                return _context.ContractOrganizations
                    .FirstOrDefault(x => x.OrganizationId == id && x.ContractId == contractId);
            }
            else
            {
                return null;
            }
        }
    }
}
