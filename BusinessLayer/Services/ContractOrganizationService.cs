using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    internal class ContractOrganizationService: IContractOrganizationService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger; 
        public ContractOrganizationService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create contract-organization, ContractID={item.ContractId}, OrganizationID=={item.OrganizationId}",
                            nameSpace: typeof(ContractOrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return contract.ContractId;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create contract-organization, object is null",
                            nameSpace: typeof(ContractOrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? contractId)
        {
            if (id > 0 && contractId != null)
            {
                var contOrg = _database.ContractOrganizations.GetById(id, contractId);

                if (contOrg is not null)
                {
                    _database.ContractOrganizations.Delete(id, contractId);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete contract-organization, ContractID={contractId}, OrganizationID=={id}",
                            nameSpace: typeof(ContractOrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete contract-organization, ID is not more than zero",
                            nameSpace: typeof(ContractOrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }           
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

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update contract-organization, ContractID={item.ContractId}, OrganizationID=={item.OrganizationId}",
                            nameSpace: typeof(ContractOrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update contract-organization, object is null",
                            nameSpace: typeof(ContractOrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<ContractOrganizationDTO> Find(Func<ContractOrganization, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ContractOrganizationDTO>>(_database.ContractOrganizations.Find(predicate));
        }
    }
}
