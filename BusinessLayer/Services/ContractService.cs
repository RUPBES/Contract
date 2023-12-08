using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Reflection.PortableExecutable;
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
                    _logger.WriteLog(LogLevel.Information, $"create contract, ID={contract.Id}, Number={contract.Number}", typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return contract.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create contract, object is null", typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

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
                        _logger.WriteLog(LogLevel.Information, $"delete contract, ID={id}", typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete contract, ID is not more than zero", typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update contract, ID={item.Id}", typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update contract, object is null", typeof(ContractService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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

        public void AddFile(int contractId, int fileId)
        {
            if (fileId > 0 && contractId > 0)
            {
                if (_database.ContractFiles.GetById(contractId, fileId) is null)
                {
                    _database.ContractFiles.Create(new ContractFile
                    {
                        ContractId = contractId,
                        FileId = fileId
                    });

                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create file of contract", typeof(ContractService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not create file of contract, object is null", typeof(ContractService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

            }
        }

        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count)
        {

            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Contract> items;
            List<Contract> itemsT = new List<Contract>();            
            if (!String.IsNullOrEmpty(request))
            {
                items = _database.Contracts.
                    Find(c => c.IsMultiple == false && c.IsEngineering == false && c.IsAgreementContract == false && c.IsOneOfMultiple == false && c.IsSubContract == false);
                foreach (var item in items)
                {
                    if (item.NameObject != null && item.NameObject.Contains(request) || item.Number != null && item.Number.Contains(request))
                        itemsT.Add(item);
                }
                items = itemsT;
                switch (filter) {
                    case "Scope": items = items.Where(i => i.ScopeWorks.Count > 0); break;
                    case "Payment": items = items.Where(i => i.Payments.Count > 0); break;
                    default: break;
                }
            }
            else { items = _database.Contracts.GetAll(); 
                switch (filter)
                {
                    case "Scope": items = items.Where(i => i.ScopeWorks.Count > 0); break;
                    case "Payment": items = items.Where(i => i.Payments.Count > 0); break;
                    default: break;
                }
            }
            count = items.Count();
            items = items.OrderBy(s => s.NameObject);                       
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<ContractDTO>>(items);            
            return t;
        }

        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum,string filter, out int count)
        {            
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Contract> items;
            items = _database.Contracts.
                Find(c => c.IsMultiple == false && c.IsEngineering == false && c.IsAgreementContract == false && c.IsOneOfMultiple == false && c.IsSubContract == false);
            switch (filter)
            {
                case "Scope": items = items.Where(x => x.ScopeWorks.Count() > 0); break;
                case "Payment": items = items.Where(x => x.Payments.Count() > 0); break;
                default: break;
            }                        
            count = items.Count();
            items = items.OrderBy(s => s.NameObject);
            items = items.Skip(skipEntities).Take(pageSize);            
            var t = _mapper.Map<IEnumerable<ContractDTO>>(items);            
            return t;
        }

    }
}