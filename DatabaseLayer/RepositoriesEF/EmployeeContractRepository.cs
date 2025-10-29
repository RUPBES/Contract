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
    internal class EmployeeContractRepository : IRepository<EmployeeContract>
    {
        private readonly ContractsContext _context;
        public EmployeeContractRepository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(EmployeeContract entity)
        {
            if (entity is not null)
            {
                _context.EmployeeContracts.Add(entity);
            }
        }

        public void Delete(int id, int? contractId = null)
        {
            EmployeeContract empContract = null;

            if (id > 0 && contractId != null)
            {
                empContract = _context.EmployeeContracts
                    .FirstOrDefault(x => x.EmployeeId == id && x.ContractId == contractId);
            }

            if (empContract is not null)
            {
                _context.EmployeeContracts.Remove(empContract);
            }
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

        public void Update(EmployeeContract entity)
        {
            if (entity is not null)
            {
                var empContract = _context.EmployeeContracts
                    .FirstOrDefault(x => x.EmployeeId == entity.EmployeeId && x.ContractId == entity.ContractId);

                if (empContract is not null)
                {
                    empContract.EmployeeId = entity.EmployeeId;
                    empContract.ContractId = entity.ContractId;
                    empContract.IsResponsible = entity.IsResponsible;
                    empContract.IsSignatory = entity.IsSignatory;

                    _context.EmployeeContracts.Update(empContract);
                }
            }
        }
    }
}
