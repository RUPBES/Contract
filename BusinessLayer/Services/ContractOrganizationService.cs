using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    internal class ContractOrganizationService: IContractOrganizationService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public ContractOrganizationService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public int? Create(ContractOrganizationDTO item)
        {
            if (item is not null)
            {
                if (_database.ContractOrganizations.GetById(item.OrganizationId, item.ContractId) is null)
                {
                    var contract = _mapper.Map<ContractOrganization>(item);

                    _database.ContractOrganizations.Create(contract);
                    _database.Save(); 
                    return contract.ContractId;
                }
            }
            return null;
        }

        public void Delete(int id, int? contractId)
        {
            _database.ContractOrganizations.Delete(id, contractId);
            _database.Save();
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public IEnumerable<ContractOrganizationDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ContractOrganizationDTO>>(_database.ContractOrganizations.GetAll());
        }

        public ContractOrganizationDTO GetById(int id, int? secondId)
        {
            var contract = _database.ContractOrganizations.GetById(id, secondId);

            if (contract is not null)
            {
                return _mapper.Map<ContractOrganizationDTO>(contract);
            }
            else
            {
                return null;
            }
        }

        public void Update(ContractOrganizationDTO item)
        {
            if (item is not null)
            {
                _database.ContractOrganizations.Update(_mapper.Map<ContractOrganization>(item));
                _database.Save();
            }
        }

        public IEnumerable<ContractOrganizationDTO> Find(Func<ContractOrganization, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ContractOrganizationDTO>>(_database.ContractOrganizations.Find(predicate));
        }
    }
}
