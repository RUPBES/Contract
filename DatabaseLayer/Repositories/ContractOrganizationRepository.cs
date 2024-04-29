using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Repositories
{
    internal class ContractOrganizationRepository : IRepository<ContractOrganization>
    {
        private readonly ContractsContext _context;
        public ContractOrganizationRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(ContractOrganization entity)
        {
            if (entity is not null)
            {
                _context.ContractOrganizations.Add(entity);
            }
        }

        public void Delete(int id, int? contractId)
        {
            ContractOrganization contractOrg = null;

            if (id > 0 && contractId != null)
            {
                contractOrg = _context.ContractOrganizations
                    .FirstOrDefault(x=>x.OrganizationId == id && x.ContractId == contractId);
            }

            if (contractOrg is not null)
            {
                _context.ContractOrganizations.Remove(contractOrg);
            }
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

        public void Update(ContractOrganization entity)
        {
            if (entity is not null)
            {
                var contractOrg = _context.ContractOrganizations
                    .FirstOrDefault(x => x.OrganizationId == entity.OrganizationId && x.ContractId == entity.ContractId);

                if (contractOrg is not null)
                {
                    contractOrg.OrganizationId = entity.OrganizationId;
                    contractOrg.ContractId = entity.ContractId;
                    contractOrg.IsClient = entity.IsClient;
                    contractOrg.IsGenContractor = entity.IsGenContractor;

                    _context.ContractOrganizations.Update(contractOrg);
                }
            }
        }
    }
}