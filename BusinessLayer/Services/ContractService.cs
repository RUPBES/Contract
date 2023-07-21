using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace BusinessLayer.Services
{
    internal class ContractService : IContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public ContractService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(ContractDTO item)
        {
            if (item is not null)
            {
                if (_database.Contracts.GetById(item.Id) is null)
                {
                    var contract = _mapper.Map<Contract>(item);

                    _database.Contracts.Create(contract);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create contract, ID={contract.Id}, Number={contract.Number}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return contract.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create contract, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var contract = _database.Contracts.GetById(id);

                if (contract is not null)
                {
                    try
                    {
                        _database.Contracts.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete contract, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete contract, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }            
        }

        public IEnumerable<ContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.GetAll());
        }

        public ContractDTO GetById(int id   , int? secondId = null)
        {
            var contract = _database.Contracts.GetById(id);

            if (contract is not null)
            {
                return _mapper.Map<ContractDTO>(contract);
            }
            else
            {
                return null;
            }
        }

        public void Update(ContractDTO item)
        {
            if (item is not null)
            {
                _database.Contracts.Update(_mapper.Map<Contract>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update contract, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update contract, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<ContractDTO> Find(Func<Contract, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.Find(predicate));
        }

        public bool ExistContractByNumber(string numberContract)
        {
            bool result = false;           

            if (_database.Contracts.Find(x => x.Number == numberContract).FirstOrDefault() is not null)
            {                
                return true;
            }

            var sameContracts = _database.Contracts.GetAll();

            string contractNumberForChecking = TrimWhitespaceIntoNumberOfContract(numberContract);

            foreach (var item in sameContracts)
            {
                if (contractNumberForChecking.Equals(TrimWhitespaceIntoNumberOfContract(item.Number), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return result;
        }

        public List<ContractDTO>? ExistContractAndReturnListSameContracts(string numberContract, DateTime? dateContract)
        {
            List<ContractDTO> contracts = new List<ContractDTO>();

            var contract = _database.Contracts.Find(x => x.Number == numberContract && x.Date == dateContract).FirstOrDefault();

            if (contract is not null)
            {
                contracts.Add(_mapper.Map<ContractDTO>(contract)); 
                return contracts;
            }

            var sameContracts = _database.Contracts.Find(x => x.Date?.ToString("yyyyMMdd") == dateContract?.ToString("yyyyMMdd"));

            string contractNumberForChecking = TrimWhitespaceIntoNumberOfContract(numberContract);

            foreach (var item in sameContracts)
            {
                if (contractNumberForChecking.Equals(TrimWhitespaceIntoNumberOfContract(item.Number), StringComparison.OrdinalIgnoreCase))
                {
                    contracts.Add(_mapper.Map<ContractDTO>(item));
                }
            }

            return contracts;
        }

        private string TrimWhitespaceIntoNumberOfContract(string numberContract)
        {
            if (string.IsNullOrWhiteSpace(numberContract))
            {
                return string.Empty;
            }
            char[] number = numberContract.ToCharArray();
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < number.Length; i++)
            {
                if (!char.IsWhiteSpace(number[i]))
                {
                    stringBuilder.Append(number[i]);
                }
            }

            return stringBuilder.ToString();
        }
    }
}