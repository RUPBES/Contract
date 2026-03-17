using DatabaseLayer.Data;
using DatabaseLayer.Interfaces.EntityFramework;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DatabaseLayer.Repositories.ARCHIVE
{
    internal class ContractArchiveRepo : IContractRepository
    {
        private readonly ContractsArchiveContext _context;
        public ContractArchiveRepo(ContractsArchiveContext context)
        {
            _context = context;
        }

        public bool CopyToArchiveDb(int contractId, string user, string sourceDB, string targetDB)
        {
            throw new NotImplementedException();
        }

        public void Create(Contract entity)
        {
            if (entity is not null)
            {
                _context.Contracts.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
        }

        public IEnumerable<Contract> Find(Func<Contract, bool> predicate)
        {
            var d = _context.Contracts
                 .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)
                .Include(p => p.Payments).
                Include(c => c.MaterialGcs).ThenInclude(c => c.MaterialCosts).Where(predicate).ToList();
            return d;
        }

        public IEnumerable<Contract> Find(Func<Contract, bool> where, Func<Contract, Contract> select)
        {
            return _context.Contracts
                //.Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee)//.ThenInclude(x => x.Phones)
                .Include(c => c.TypeWorkContracts).ThenInclude(o => o.TypeWork)
                .Where(where).Select(select).ToList();
        }

        public IEnumerable<Contract> GetAll()
        {
            return _context.Contracts
                .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ContractOrganizations).ThenInclude(o => o.Organization)
                .Include(c => c.TypeWorkContracts).ThenInclude(o => o.TypeWork)
                .Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee).ThenInclude(x => x.Phones)
                .Include(c => c.SelectionProcedures)
                .Include(c => c.Acts)
                .Include(c => c.CommissionActs)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)
                .Include(c => c.Payments)
                .ToList();
        }

        public Contract GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Contracts.Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ContractOrganizations).ThenInclude(o => o.Organization)
                .Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee).ThenInclude(x => x.Phones)
                .Include(c => c.SelectionProcedures)
                .Include(c => c.Acts)
                .Include(c => c.CommissionActs)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)
                .FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public bool RemoveContractData(int contractId, string user, string targetDB = "ContrArchiveTest")
        {
            throw new NotImplementedException();
        }

        public void Update(Contract entity)
        {
        }
    }
}
