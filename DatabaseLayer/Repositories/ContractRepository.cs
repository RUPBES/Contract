using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .ToList();
        }

        public Contract GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.Contracts.Find(id);
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

                    _context.Contracts.Update(contract);
                }
            }
        }
    }
}