using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using Contract = DatabaseLayer.Models.KDO.Contract;

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
                    var contract = _mapper.Map<Contract>(item);

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

        public IEnumerable<ContractDTO> Find(Func<Contract, bool> where, Func<Contract, Contract> select)
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.Find(where, select));
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
            var list = org.Split(',');
            Func<Contract, bool> where;
            Func<Contract, string> orderBy = o => o.NameObject;
            Func<Contract, Contract> select = s => new Contract
            {
                NameObject = s.NameObject,
                Number = s.Number,
                Date = s.Date,
                Id = s.Id,
                DateBeginWork = s.DateBeginWork,
                DateEndWork = s.DateEndWork,
                EnteringTerm = s.EnteringTerm,
                Сurrency = s.Сurrency,
                ContractPrice = s.ContractPrice
            };
            int skipEntities = (pageNum - 1) * pageSize;
            if (!String.IsNullOrEmpty(request))
            {
                where = w => w.IsEngineering == false &&
                w.IsAgreementContract == false &&
                w.IsOneOfMultiple == false &&
                w.IsSubContract == false &&
                list.Contains(w.Owner) &&
                (w.NameObject.Contains(request) || w.Number.Contains(request));
            }
            else
            {
                where = w => w.IsEngineering == false &&
                w.IsAgreementContract == false &&
                w.IsOneOfMultiple == false &&
                w.IsSubContract == false &&
                 list.Contains(w.Owner);
            }
            IEnumerable<Contract> items = _database.Contracts.Find(where: where, select: select).OrderBy(o => o.NameObject);
            count = items.Count();
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<ContractDTO>>(items);
            return t;
        }

        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org)
        {
            var list = org.Split(',');
            Func<Contract, bool> where = w => w.IsEngineering == false &&
                w.IsAgreementContract == false &&
                w.IsOneOfMultiple == false &&
                w.IsSubContract == false &&
                list.Contains(w.Owner);
            Func<Contract, Contract> select = s => new Contract
            {
                NameObject = s.NameObject,
                Number = s.Number,
                Date = s.Date,
                Id = s.Id,
                DateBeginWork = s.DateBeginWork,
                DateEndWork = s.DateEndWork,
                EnteringTerm = s.EnteringTerm,
                Сurrency = s.Сurrency,
                ContractPrice = s.ContractPrice
            };
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Contract> items = _database.Contracts.Find(where, select);

            count = items.Count();
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
                    var daysStr = raschet.Split(' ')[0];
                    int answer;
                    var isParse = int.TryParse(daysStr, out answer);
                    if (isParse)
                    {
                        return answer;
                    }
                }
            }
            return null;
        }

        public bool IsNotGenContract(int? contractId, out int mainContrId)
        {
            mainContrId = 0;
            var contract = contractId.HasValue ? _database.Contracts.GetById((int)contractId) : null;

            if ((contract?.IsAgreementContract ?? false))
            {
                mainContrId = contract?.AgreementContractId ?? 0;
            }
            else if ((contract?.IsSubContract ?? false))
            {
                mainContrId = contract?.SubContractId ?? 0;
            }
            else if (contract?.IsOneOfMultiple ?? false)
            {
                mainContrId = contract?.MultipleContractId ?? 0;
            }

            return (contract?.IsOneOfMultiple ?? false) || (contract?.IsSubContract ?? false) || (contract?.IsAgreementContract ?? false);
        }

        public ContractType GetContractType(ContractDTO? contract, out int parentContrId)
        {
            parentContrId = 0;

            if ((contract?.IsAgreementContract ?? false))
            {
                parentContrId = contract?.AgreementContractId ?? 0;
                return ContractType.Agreement;
            }
            else if ((contract?.IsSubContract ?? false))
            {
                parentContrId = contract?.SubContractId ?? 0;
                return ContractType.SubContract;
            }
            else if (contract?.IsOneOfMultiple ?? false)
            {
                parentContrId = contract?.MultipleContractId ?? 0;
                return ContractType.MultipleContract;
            }

            return ContractType.GenСontract;
        }

        public Dictionary<int, ContractType>? GetParentsList(ContractDTO? contract)
        {
            var listParents = new Dictionary<int, ContractType>();
            int parentId = contract.Id;


            if ((contract?.IsAgreementContract ?? false))
            {
                parentId = _database.Contracts.GetById(contract?.AgreementContractId ?? 0).Id;
                contract = _mapper.Map<ContractDTO>(_database.Contracts.GetById(parentId));
            }
            else if ((contract?.IsSubContract ?? false))
            {
                parentId = _database.Contracts.GetById(contract?.SubContractId ?? 0).Id;
                contract = _mapper.Map<ContractDTO>(_database.Contracts.GetById(parentId));
            }
            else if (contract?.IsOneOfMultiple ?? false)
            {
                parentId = _database.Contracts.GetById(contract?.MultipleContractId ?? 0).Id;
                contract = _mapper.Map<ContractDTO>(_database.Contracts.GetById(parentId));
            }
            else
            {
                listParents.Add(contract?.Id ?? 0, ContractType.GenСontract);
                parentId = 0;
            }


            while (parentId > 0)
            {
                if ((contract?.IsAgreementContract ?? false))
                {
                    listParents.Add(contract?.Id ?? 0, ContractType.Agreement);
                    parentId = _database.Contracts.GetById(contract?.AgreementContractId ?? 0).Id;
                    contract = _mapper.Map<ContractDTO>(_database.Contracts.GetById(parentId));
                }
                else if ((contract?.IsSubContract ?? false))
                {
                    listParents.Add(contract?.Id ?? 0, ContractType.SubContract);
                    parentId = _database.Contracts.GetById(contract?.SubContractId ?? 0).Id;
                    contract = _mapper.Map<ContractDTO>(_database.Contracts.GetById(parentId));
                }
                else if (contract?.IsOneOfMultiple ?? false)
                {
                    listParents.Add(contract?.Id ?? 0, ContractType.MultipleContract);
                    parentId = _database.Contracts.GetById(contract?.MultipleContractId ?? 0).Id;
                    contract = _mapper.Map<ContractDTO>(_database.Contracts.GetById(parentId));
                }
                else 
                {
                    listParents.Add(contract?.Id ?? 0, ContractType.GenСontract);
                    break;
                } 
            }

            return listParents;
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


        public bool IsThereAmendment(int contarctId)
        {
            var amendmentId = _database.Amendments.Find(x => x.ContractId == contarctId).LastOrDefault()?.Id;

            if (amendmentId is not null && amendmentId > 0)
            {
                return true;
            }
            return false;
        }

        public (bool isExistChild, int id) IsHaveChild(int id)
        {

            return (true, 0);
        }
    }
}