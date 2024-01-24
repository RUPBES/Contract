using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using Contract = DatabaseLayer.Models.Contract;

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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            if (item is not null)
            {
                if (_database.Contracts.GetById(item.Id) is null)
                {
                    var contract = _mapper.Map<DatabaseLayer.Models.Contract>(item);

                    _database.Contracts.Create(contract);
                    _database.Save();

                    _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"create contract, ID={contract.Id}, Number={contract.Number}",
                        nameSpace: typeof(ContractService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name,
                        userName: user);
                    return contract.Id;
                }
            }
            _logger.WriteLog(
                logLevel: LogLevel.Warning,
                message: $"not create contract, object is null",
                nameSpace: typeof(ContractService).Name,
                methodName: MethodBase.GetCurrentMethod().Name,
                userName: user);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            if (id > 0)
            {
                var contract = _database.Contracts.GetById(id);

                if (contract is not null)
                {
                    try
                    {
                        _database.Contracts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete contract, ID={contract.Id}",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                    logLevel: LogLevel.Warning,
                    message: $"not delete contract, ID is not more than zero",
                    nameSpace: typeof(ContractService).Name,
                    methodName: MethodBase.GetCurrentMethod().Name,
                    userName: user);
            }
        }

        public void DeleteAfterScopeWork(int id)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            if (id > 0)
            {
                var contract = _database.Contracts.GetById(id);

                if (contract is not null)
                {
                    try
                    {
                        var scopes = _database.ScopeWorks.Find(x => x.ContractId == id);

                        foreach (var item in scopes)
                        {
                            foreach (var item1 in item.SWCosts)
                            {
                                _database.SWCosts.Delete(item1.Id);
                            }
                            _database.Save();
                        }

                        _database.Contracts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete contract, ID={id}",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete contract, ID is not more than zero",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public void DeleteScopeWorks(int id)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            if (id > 0)
            {
                var contract = _database.Contracts.GetById(id);

                if (contract is not null)
                {
                    try
                    {
                        var scopes = _database.ScopeWorks.Find(x => x.ContractId == id);

                        foreach (var item in scopes)
                        {
                            foreach (var item1 in item.SWCosts)
                            {
                                _database.SWCosts.Delete(item1.Id);
                            }
                            _database.ScopeWorks.Delete(item.Id);
                        }

                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete contract's the scope works, ID={id}",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete contract, ID is not more than zero",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<ContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.GetAll());
        }

        public ContractDTO GetById(int id, int? secondId = null)
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            if (item is not null)
            {
                _database.Contracts.Update(_mapper.Map<Contract>(item));
                _database.Save();
                _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"update contract, ID={item.Id}",
                           nameSpace: typeof(ContractService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: $"not update contract, object is null",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var list = _database.Contracts.GetAll();
            var contract = list.Where(x => x.Number != null && x.Number == numberContract && x.Date != null && x.Date == dateContract).FirstOrDefault();

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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
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

                    _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"create file of contract",
                           nameSpace: typeof(ContractService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
                }
            }
            else
            {
                _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not create file of contract, object is null",
                           nameSpace: typeof(ContractService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user); 
            }
        }

        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org)
        {
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Contract> items;
            List<Contract> itemsT = new List<Contract>();
            if (!String.IsNullOrEmpty(request))
            {
                items = _database.Contracts.
                    Find(c => c.IsEngineering == false && c.IsAgreementContract == false
                    && c.IsOneOfMultiple == false && c.IsSubContract == false &&
                    (c.Author == org || c.Owner == org));
                foreach (var item in items)
                {
                    if (item.NameObject != null && item.NameObject.Contains(request) || item.Number != null && item.Number.Contains(request))
                        itemsT.Add(item);
                }
                items = itemsT;
                switch (filter)
                {
                    case "Scope": items = items.Where(i => i.ScopeWorks.Count > 0); break;
                    case "Payment": items = items.Where(i => i.Payments.Count > 0); break;
                    case "Material": items = items.Where(i => i.MaterialGcs.Count > 0); break;
                    default: break;
                }
            }
            else
            {
                items = _database.Contracts.GetAll();
                switch (filter)
                {
                    case "Scope": items = items.Where(i => i.ScopeWorks.Count > 0); break;
                    case "Payment": items = items.Where(i => i.Payments.Count > 0); break;
                    case "Material": items = items.Where(i => i.MaterialGcs.Count > 0); break;
                    default: break;
                }
            }
            count = items.Count();
            items = items.OrderBy(s => s.NameObject);
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<ContractDTO>>(items);
            return t;
        }

        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org)
        {
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Contract> items;
            items = _database.Contracts.
                Find(c => c.IsEngineering == false && c.IsAgreementContract == false && c.IsOneOfMultiple == false
                && c.IsSubContract == false &&
                    (c.Author == org || c.Owner == org));
            switch (filter)
            {
                case "Scope": items = items.Where(x => x.ScopeWorks.Count() > 0); break;
                case "Payment": items = items.Where(x => x.Payments.Count() > 0); break;
                case "Material": items = items.Where(i => i.MaterialGcs.Count > 0); break;
                default: break;
            }
            count = items.Count();
            items = items.OrderBy(s => s.NameObject);
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<ContractDTO>>(items);
            return t;
        }

        public int? GetDayOfRaschet(int contrId)
        {
            if (contrId > 0)
            {
                var raschet = _database.Contracts.GetById(contrId).PaymentСonditionsRaschet;
                if (raschet is not null)
                {
                    raschet = raschet.Replace("Расчет за выполненные работы производится в течение ", "");
                    raschet = raschet.Replace("Расчет за выполненные работы производится не позднее ", "");
                    var daysStr =raschet.Split(' ')[0];
                    int answer;
                    var isParse = int.TryParse(daysStr, out answer);
                    if (isParse) return answer;
                    else return 0;                    
                }
            }
            return null;
        }

        public bool IsThereSubObjs(int contarctId)
        {
            var subObjs = _database.Contracts.Find(x => x.IsOneOfMultiple == true && x.MultipleContractId == contarctId);

            if (subObjs is not null && subObjs.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public bool IsThereScopeWorks(int contarctId, out int? scopeId)
        {
            scopeId = _database.ScopeWorks.Find(x => x.ContractId == contarctId).LastOrDefault()?.Id;

            if (scopeId is not null && scopeId > 0)
            {
                return true;
            }
            return false;
        }

        public bool IsThereScopeWorks(int contarctId, bool isOwnForses, out int? scopeId)
        {
            scopeId = _database.ScopeWorks.Find(x => x.ContractId == contarctId && x.IsOwnForces == isOwnForses).LastOrDefault()?.Id;

            if (scopeId is not null && scopeId > 0)
            {
                    return true;
            }
            return false;
        }

        public bool IsThereSWCosts(int? scopeId)
        {           
            if (scopeId is not null && scopeId > 0)
            {
                if (_database.SWCosts.Find(x=>x.ScopeWorkId == scopeId)?.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsThereAmendment(int contarctId)
        {
            var amendmentId = _database.Amendments.Find(x => x.ContractId == contarctId).LastOrDefault()?.Id;

            if (amendmentId is not null && amendmentId > 0)
            {               
                    return true;
            }
            return false;
        }
    }
}