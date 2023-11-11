using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLayer.Repositories
{
    internal class ContractRepository : IRepository<Contract>
    {
        private readonly ContractsContext _context;
        public ContractRepository(ContractsContext context)
        {
            _context = context;
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
            Contract contract = null;

            if (id > 0)
            {
                contract = _context.Contracts.Find(id);
            }

            if (contract is not null)
            {
                _context.Contracts.Remove(contract);
            }
        }

        public IEnumerable<Contract> Find(Func<Contract, bool> predicate)
        {
            return _context.Contracts.Where(predicate).ToList();
        }

        public IEnumerable<Contract> GetAll()
        {
            return _context.Contracts
                .Include(c => c.AgreementContract)
                .Include(c => c.SubContract)
                .Include(c => c.ContractOrganizations).ThenInclude(o => o.Organization)
                .Include(c => c.EmployeeContracts).ThenInclude(o => o.Employee).ThenInclude(x=>x.Phones)
                .Include(c => c.SelectionProcedures)
                .Include(c => c.Acts)
                .Include(c => c.CommissionActs)
                .Include(c => c.ScopeWorks).ThenInclude(o => o.SWCosts)
                .Include(c => c.FormC3as)
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
                .Include(c => c.FormC3as)
                .FirstOrDefault(x=>x.Id == id);
            }
            else
            {
                return null;
            }
        }

        public void Update(Contract entity)
        {
            if (entity is not null)
            {
                var contract = _context.Contracts.Find(entity.Id);

                if (contract is not null)
                {
                    contract.Number = entity.Number;
                    contract.SubContractId = entity.SubContractId;
                    contract.AgreementContractId = entity.AgreementContractId;
                    contract.Date = entity.Date;
                    contract.EnteringTerm = entity.EnteringTerm;
                    contract.ContractTerm = entity.ContractTerm;

                    contract.DateBeginWork = entity.DateBeginWork;
                    contract.DateEndWork = entity.DateEndWork;
                    contract.Сurrency = entity.Сurrency;
                    contract.ContractPrice = entity.ContractPrice;
                    contract.NameObject = entity.NameObject;
                    contract.PaymentСonditionsAvans = entity.PaymentСonditionsAvans;
                    contract.PaymentСonditionsRaschet = entity.PaymentСonditionsRaschet;

                    contract.FundingSource = entity.FundingSource;
                    contract.IsSubContract = entity.IsSubContract;
                    contract.IsEngineering = entity.IsEngineering;
                    contract.IsAgreementContract = entity.IsAgreementContract;  
                    
                    foreach (var item in entity.ContractOrganizations)
                    {
                        var target = item.IsGenContractor != null ? "Gen" : "Client";
                        var sameObject = _context.ContractOrganizations.Where(x => 
                        x.ContractId == item.ContractId &&
                        x.OrganizationId == item.OrganizationId).FirstOrDefault();
                        ContractOrganization? objectWithSameContractandType = new ContractOrganization();
                        objectWithSameContractandType = item.IsGenContractor != null ?
                            _context.ContractOrganizations.Where(x =>
                            x.ContractId == item.ContractId &&
                            x.IsGenContractor == item.IsGenContractor).
                            FirstOrDefault()
                            :
                            _context.ContractOrganizations.Where(x =>
                            x.ContractId == item.ContractId &&
                            x.IsClient == item.IsClient).
                            FirstOrDefault();
                        if (sameObject != null)
                        {
                            if (objectWithSameContractandType != null)
                            {
                                _context.ContractOrganizations.Remove(objectWithSameContractandType);                                
                            }
                            switch (target)
                            {
                                case "Gen":
                                    sameObject.IsGenContractor = true; break;
                                case "Client":
                                    sameObject.IsClient = true; break;
                            }
                            _context.ContractOrganizations.Update(sameObject);
                        }
                        else if (objectWithSameContractandType != null)
                        {
                            if (objectWithSameContractandType.IsClient == true && objectWithSameContractandType.IsGenContractor == true)
                            {
                                switch (target)
                                {
                                    case "Gen":
                                        objectWithSameContractandType.IsGenContractor = false; break;
                                    case "Client":
                                        objectWithSameContractandType.IsClient = false; break;
                                }
                                _context.ContractOrganizations.Update(objectWithSameContractandType);
                                _context.ContractOrganizations.Add(item);
                            }
                            else
                            {
                                _context.ContractOrganizations.Remove(objectWithSameContractandType);
                                _context.SaveChanges(); 
                                objectWithSameContractandType.OrganizationId = item.OrganizationId;
                                _context.ContractOrganizations.Add(objectWithSameContractandType);
                            }
                        }
                        else { _context.ContractOrganizations.Add(item); }
                        _context.SaveChanges();
                    }                                   

                    _context.Contracts.Update(contract);
                }
            }
        }
    }
}