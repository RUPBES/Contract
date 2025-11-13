using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using Contract = DatabaseLayer.Models.KDO.Contract;

namespace BusinessLayer.Services
{
    internal class ContractService : IContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly ILoggerContract _logger;
     

        public ContractService(IContractUoW database, IMapper mapper, ILoggerContract logger, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _databaseArch = databaseArch;           
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

                    _logger.WriteLog(
                        logLevel: LogLevel.Information,
                        message: $"create contract, ID={contract.Id}, Number={contract.Number}",
                        nameSpace: typeof(ContractService).Name,
                        methodName: MethodBase.GetCurrentMethod().Name);
                    return contract.Id;
                }
            }
            _logger.WriteLog(
                logLevel: LogLevel.Warning,
                message: $"not create contract, object is null",
                nameSpace: typeof(ContractService).Name,
                methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public ContractDTO GetById(int id, int? secondId)
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

        public ContractDTO GetById(int id, int? secondId, bool useArchiveData)
        {

            var contract = useArchiveData ?
                                _databaseArch.Contracts.GetById(id)
                                : _database.Contracts.GetById(id);

            if (contract is not null)
            {
                return _mapper.Map<ContractDTO>(contract);
            }
            else
            {
                return new();
            }
        }

        public IEnumerable<ContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.GetAll());
        }

        public IEnumerable<ContractDTO> Find(Func<Contract, bool> predicate, bool? useArchiveData)
        {
            return (useArchiveData == true) ?
                _mapper.Map<IEnumerable<ContractDTO>>(_databaseArch.Contracts.Find(predicate)) :
                _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.Find(predicate));
        }

        public IEnumerable<ContractDTO> Find(Func<Contract, bool> where, Func<Contract, Contract> select, bool useArchiveData)
        {
            return (useArchiveData == true) ?
                 _mapper.Map<IEnumerable<ContractDTO>>(_databaseArch.Contracts.Find(where, select)) :
                 _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.Find(where, select));
        }

        public void Update(ContractDTO item)
        {
            if (item is not null)
            {
                _database.Contracts.Update(_mapper.Map<Contract>(item));
                _database.Save();
                _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"update contract, ID={item.Id}",
                           nameSpace: typeof(ContractService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: $"not update contract, object is null",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
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
                        #region Дочерние контракты
                        var contracts = _database.Contracts.Find(x => x.MultipleContractId == id ||
                        x.SubContractId == id ||
                        x.AgreementContractId == id).ToList();
                        foreach (var item in contracts)
                            Delete(item.Id);
                        #endregion
                        #region Процедура выбора
                        var selectProcedure = _database.SelectionProcedures.Find(x => x.ContractId == id).ToList();
                        foreach (var item in selectProcedure)
                            _database.SelectionProcedures.Delete(item.Id);
                        #endregion
                        #region Сметы
                        var estimates = _database.Estimates.Find(x => x.ContractId == id).ToList();
                        #region Файлы сметы
                        foreach (var item in estimates)
                        {
                            var filesId = _database.EstimateFiles.Find(x => x.EstimateId == item.Id).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in estimates)
                            _database.Estimates.Delete(item.Id);
                        #endregion
                        #region Переписка с заказчиком
                        var correspondences = _database.Correspondences.Find(x => x.ContractId == id).ToList();
                        #region Файлы перепискы с заказчиком
                        foreach (var item in correspondences)
                        {
                            var filesId = _database.CorrespondenceFiles.Find(x => x.CorrespondenceId == item.Id).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in correspondences)
                            _database.Correspondences.Delete(item.Id);
                        #endregion
                        #region Проектно-сметная документация
                        var estimateDocs = _database.EstimateDocs.Find(x => x.ContractId == id).ToList();
                        #region Файлы псд
                        foreach (var item in estimateDocs)
                        {
                            var filesId = _database.EstimateDocFiles.Find(x => x.EstimateDocId == item.Id).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in estimateDocs)
                            _database.EstimateDocs.Delete(item.Id);
                        #endregion
                        #region Акт приостановки/возобновления работ
                        var acts = _database.Acts.Find(x => x.ContractId == id).ToList();
                        #region Файлы акта приост./возобн. работ
                        foreach (var item in acts)
                        {
                            var filesId = _database.ActFiles.Find(x => x.ActId == item.Id).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in acts)
                            _database.Acts.Delete(item.Id);
                        #endregion
                        #region Формы С3А
                        var forms = _database.Forms.Find(x => x.ContractId == id).ToList();
                        #region Файлы формы С3А
                        foreach (var item in forms)
                        {
                            var filesId = _database.FormFiles.Find(x => x.FormId == item.Id).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in forms)
                            _database.Forms.Delete(item.Id);
                        #endregion
                        #region Акт ввода
                        var commissionActs = _database.CommissionActs.Find(x => x.ContractId == id).ToList();
                        #region Файлы акта ввода
                        foreach (var item in commissionActs)
                        {
                            var filesId = _database.CommissionActFiles.Find(x => x.СommissionActId == item.Id).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in commissionActs)
                            _database.CommissionActs.Delete(item.Id);
                        #endregion
                        #region Услуги генподряда
                        var serviceGCs = _database.ServiceGCs.Find(x => x.ContractId == id).ToList();
                        #region Связь изменнений и услуг
                        foreach (var item in serviceGCs)
                        {
                            var amends = _database.ServiceAmendments.Find(x => x.ServiceId == item.Id).ToList();
                            foreach (var file in amends)
                                _database.ServiceAmendments.Delete(file.ServiceId, file.AmendmentId);
                        }
                        #endregion
                        foreach (var item in serviceGCs)
                            _database.ServiceGCs.Delete(item.Id);
                        #endregion
                        #region Объем работы
                        var scopeWorks = _database.ScopeWorks.Find(x => x.ContractId == id).ToList();
                        #region Связь изменений и объема работ
                        foreach (var item in scopeWorks)
                        {
                            var amends = _database.ScopeWorkAmendments.Find(x => x.ScopeWorkId == item.Id).ToList();
                            foreach (var file in amends)
                                _database.ScopeWorkAmendments.Delete(file.ScopeWorkId, file.AmendmentId);
                        }
                        #endregion
                        foreach (var item in scopeWorks)
                        {
                            var swcosts = _database.SWCosts.Find(x => x.ScopeWorkId == item.Id).ToList();
                            foreach (var swcost in swcosts)
                                _database.SWCosts.Delete(swcost.Id);
                            _database.ScopeWorks.Delete(item.Id);
                        }
                        #endregion
                        #region Материалы генподрядчика
                        var materials = _database.Materials.Find(x => x.ContractId == id).ToList();
                        #region Связь изменений и материалов генподрядчика
                        foreach (var item in materials)
                        {
                            var amends = _database.MaterialAmendments.Find(x => x.MaterialId == item.Id).ToList();
                            foreach (var file in amends)
                                _database.MaterialAmendments.Delete(file.MaterialId, file.AmendmentId);
                        }
                        #endregion
                        foreach (var item in materials)
                            _database.Materials.Delete(item.Id);
                        #endregion
                        #region Авансы
                        var prepayments = _database.Prepayments.Find(x => x.ContractId == id).ToList();
                        #region Связь изменений и авансов, файлы аввнсов
                        foreach (var item in prepayments)
                        {
                            var amends = _database.PrepaymentAmendments.Find(x => x.PrepaymentId == item.Id).ToList();
                            foreach (var file in amends)
                                _database.PrepaymentAmendments.Delete(file.PrepaymentId, file.AmendmentId);
                            var prepaymentTakes = _database.PrepaymentTakes.Find(x => x.PrepaymentId == item.Id).ToList();
                            foreach (var prepayment in prepaymentTakes)
                                _database.Files.Delete((int)prepayment.FileId);
                        }
                        #endregion
                        foreach (var item in prepayments)
                            _database.Prepayments.Delete(item.Id);
                        #endregion
                        #region Изменения к договору                        
                        var amendsId = _database.Amendments.Find(x => x.ContractId == id).Select(x => x.Id).ToList();
                        #region Файлы изменений к договору  
                        foreach (var item in amendsId)
                        {
                            var filesId = _database.FormFiles.Find(x => x.FormId == item).Select(x => x.FileId).ToList();
                            foreach (var file in filesId)
                                _database.Files.Delete(file);
                        }
                        #endregion
                        foreach (var item in amendsId)
                            _database.Amendments.Delete(item);
                        #endregion
                        #region Файлы к договору 
                        var files = _database.ContractFiles.Find(x => x.ContractId == id).Select(x => x.FileId).ToList();
                        foreach (var file in files)
                            _database.Files.Delete(file);
                        #endregion
                        _database.Contracts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete contract, ID={contract.Id}",
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ContractService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                    logLevel: LogLevel.Warning,
                    message: $"not delete contract, ID is not more than zero",
                    nameSpace: typeof(ContractService).Name,
                    methodName: MethodBase.GetCurrentMethod().Name);
            }
        }


        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org, bool useArchiveData)
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
            IEnumerable<Contract> items = useArchiveData ?
                                            _databaseArch.Contracts.Find(where, select)
                                            : _database.Contracts.Find(where, select);

            count = items.Count();
            items = items.Skip(skipEntities).Take(pageSize);
            return _mapper.Map<IEnumerable<ContractDTO>>(items);
        }

        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org, bool useArchiveData)
        {
            var list = org.Split(',');
            int skipEntities = (pageNum - 1) * pageSize;

            Func<Contract, bool> where;
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

            IEnumerable<Contract> items = useArchiveData ?
                _databaseArch.Contracts.Find(where: where, select: select).OrderBy(o => o.NameObject) :
                _database.Contracts.Find(where: where, select: select).OrderBy(o => o.NameObject);

            count = items.Count();
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<ContractDTO>>(items);
            return t;
        }

        public async Task<bool> MoveToArchive(int contrId, string user, string sourceDB = "ContrTest", string targetDB = "ContrArchiveTest")
        {
            return await Task.Run(() =>
             {
                 bool isSuccessCopy = false;
                 var childrenContracts = GetChildren(contrId);
                 isSuccessCopy = _database.Contracts.CopyToArchiveDb(contrId, user, sourceDB, targetDB);

                 if (childrenContracts.Count > 0 && isSuccessCopy)
                 {
                     foreach (var childId in childrenContracts)
                     {
                         isSuccessCopy = _database.Contracts.CopyToArchiveDb(childId, user, sourceDB, targetDB);
                     }
                 }

                 if (isSuccessCopy)
                 {
                     _logger.WriteLog(
                       logLevel: LogLevel.Information,
                       message: $"contract was archived successful, ID={contrId}",
                       nameSpace: typeof(ContractService).Name,
                       methodName: MethodBase.GetCurrentMethod().Name);

                     childrenContracts.Reverse();
                     foreach (var childId in childrenContracts)
                     {
                         isSuccessCopy = _database.Contracts.RemoveArchivedContractData(childId, user, sourceDB);
                     }
                     isSuccessCopy = _database.Contracts.RemoveArchivedContractData(contrId, user, sourceDB);
                 }
                 if (isSuccessCopy)
                 {
                     _logger.WriteLog(
                      logLevel: LogLevel.Information,
                      message: $"contract was deleted successful from a source DB",
                      nameSpace: typeof(ContractService).Name,
                      methodName: MethodBase.GetCurrentMethod().Name);
                 }
                 return isSuccessCopy;
             });
        }

        public async Task<int> Restructure(int contrId, string user)
        {
            return await Task.Run(() =>
            {
                int? subobjId;
                subobjId = _database.Contracts.SplitGenContract(contrId, user);

                if (subobjId.HasValue)
                {
                    _logger.WriteLog(
                    logLevel: LogLevel.Information,
                    message: $"contract was divided successful",
                    nameSpace: typeof(ContractService).Name,
                    methodName: MethodBase.GetCurrentMethod().Name);
                    return subobjId.Value;
                }
                else
                {
                    _logger.WriteLog(
                     logLevel: LogLevel.Information,
                     message: $"contract was not divided",
                     nameSpace: typeof(ContractService).Name,
                     methodName: MethodBase.GetCurrentMethod().Name);
                    return 0;
                }
               
            });
        }

        /// <summary>
        /// Возвращает список договоров, принадлежащих генподрядному договору по его ID и 
        /// типу необходимых договоров
        /// </summary>
        /// <param name="id">ID Гендоговора</param>
        /// <param name="contractType">Тип договора, который необходимо найти (Соглашение, субподряд, подобъект))</param>
        /// <returns>список вложенных договоров принадлежащих генподрядному</returns>
        public IEnumerable<ContractDTO> GetSubsByType(int? id, Enums.Contract? contractType, bool useArchiveData)
        {
            if (!id.HasValue || contractType == null)
            {
                return Enumerable.Empty<ContractDTO>();
            }
            Func<Contract, bool> selector;

            if (contractType == Enums.Contract.SubContract)
            {
                selector = x => x.SubContractId == id && x.IsSubContract == true;
            }
            else if (contractType == Enums.Contract.Agreement)
            {
                selector = x => x.AgreementContractId == id && x.IsAgreementContract == true;
            }
            else if (contractType == Enums.Contract.MultipleContract)
            {
                selector = x => x.MultipleContractId == id && x.IsOneOfMultiple == true;
            }
            else
            {
                return Enumerable.Empty<ContractDTO>();
            }

            var contracts = useArchiveData ?
                _databaseArch.Contracts.Find(selector) :
                _database.Contracts.Find(selector);

            if (contracts.Any())
            {
                foreach (var item in contracts)
                {
                    var amend = useArchiveData ?
                                _databaseArch.Amendments.Find(x => x.ContractId == item.Id).ToList() :
                                _database.Amendments.Find(x => x.ContractId == item.Id).ToList();

                    if (amend.Count > 0)
                    {
                        amend = amend.OrderBy(x => x.Date).ToList();
                        item.ContractPrice = amend.Last().ContractPrice;
                    }
                }
                return _mapper.Map<IEnumerable<ContractDTO>>(contracts);
            }
            else
            {
                return Enumerable.Empty<ContractDTO>();
            }
        }



        /// <summary>
        /// Возвращает количество дней после заключения договора для расчета
        /// </summary>
        /// <param name="contrId">ID договора</param>
        /// <returns>количество дней</returns>
        public int? GetPaymentDueDate(int contrId, bool? useArchiveData)
        {
            if (contrId > 0)
            {
                var raschet = (useArchiveData == true)?
                    _databaseArch.Contracts.GetById(contrId)?.PaymentСonditionsRaschet:
                    _database.Contracts.GetById(contrId)?.PaymentСonditionsRaschet;

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

        /// <summary>
        /// Возвращает количество дней после заключения договора для расчета
        /// </summary>
        /// <param name="paymentDescription">строка с полным названием расчета</param>
        /// <param name="subPaymentDescription"> часть названия, которое должно содержаться в строке с полным названием</param>
        /// <returns>количество дней</returns>
        public int? GetPaymentDueDate(string? paymentDescription, string? subPaymentDescription)
        {
            if (string.IsNullOrEmpty(paymentDescription) || string.IsNullOrEmpty(subPaymentDescription))
            {
                return null;
            }

            bool? isSamePaymentment = paymentDescription?.Contains(subPaymentDescription);

            if (isSamePaymentment == true)
            {
                var raschet = paymentDescription?.Replace(subPaymentDescription, "").TrimEnd().Split(" ").LastOrDefault();
                int answer;
                var isParse = int.TryParse(raschet, out answer);
                if (isParse)
                {
                    return answer;
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает true если номер договора уже существует, если нет - false
        /// </summary>
        /// <param name="contractNumber">Номер договора для проверки</param>
        /// <returns></returns>
        public bool IsContractNumberExists(string contractNumber)
        {
            contractNumber = contractNumber?.Replace(" ", "") ?? string.Empty;
            return _database.Contracts.Find(x => x.Number != null && x.Number?.Replace(" ", "")?.Equals(contractNumber) == true).Any();
        }

        /// <summary>
        /// Возвращает список "родительских" договоров, если они отсутствуют возвращает генподрядный договор
        /// </summary>
        /// <param name="contractId">ID договора, для которого проверяем "родительские" договора</param>
        /// <returns>Коллекция "родительских" договоров, Ключ = ID договора,  Значение = Тип договора</returns>
        public Dictionary<int, Enums.Contract>? GetParents(int? contractId, out Enums.Contract thisType)
        {
            var listParents = new Dictionary<int, Enums.Contract>();
            thisType = Enums.Contract.GenСontract;
            int parentId = contractId ?? 0;
            var contractProps = GetContractTypingProps(parentId);

            if (contractProps?.IsAgreementContract ?? false)
            {
                parentId = contractProps?.AgreementContractId ?? 0;
                contractProps = GetContractTypingProps(parentId);
                thisType = Enums.Contract.Agreement;
            }
            else if (contractProps?.IsSubContract ?? false)
            {
                parentId = contractProps?.SubContractId ?? 0;
                contractProps = GetContractTypingProps(parentId);
                thisType = Enums.Contract.SubContract;
            }
            else if (contractProps?.IsOneOfMultiple ?? false)
            {
                parentId = contractProps?.MultipleContractId ?? 0;
                contractProps = GetContractTypingProps(parentId);
                thisType = Enums.Contract.MultipleContract;
            }
            else
            {
                //return new ();
                listParents.Add(parentId, Enums.Contract.GenСontract);
                parentId = 0;
            }


            while (parentId > 0)
            {
                if ((contractProps?.IsAgreementContract ?? false))
                {
                    listParents.Add(parentId, Enums.Contract.Agreement);
                    parentId = contractProps?.AgreementContractId ?? 0;
                    contractProps = GetContractTypingProps(parentId);
                }
                else if ((contractProps?.IsSubContract ?? false))
                {
                    listParents.Add(parentId, Enums.Contract.SubContract);
                    parentId = contractProps?.SubContractId ?? 0;
                    contractProps = GetContractTypingProps(parentId);
                }
                else if (contractProps?.IsOneOfMultiple ?? false)
                {
                    listParents.Add(parentId, Enums.Contract.MultipleContract);
                    parentId = contractProps?.MultipleContractId ?? 0;
                    contractProps = GetContractTypingProps(parentId);
                }
                else
                {
                    listParents.Add(parentId, Enums.Contract.GenСontract);
                    break;
                }
            }

            return listParents;
        }

        /// <summary>
        /// Возвращает список ID всех дочерних договоров для указанного договора
        /// </summary>
        /// <param name="contractId">ID договора, для которого ищем дочерние договоры</param>
        /// <returns>Список ID всех дочерних договоров</returns>
        public List<int> GetChildren(int contractId)
        {
            var childIds = new List<int>();
            var processedIds = new HashSet<int>();
            GetChildrenRecursive(contractId, childIds, processedIds);
            return childIds;
        }




        private dynamic? GetContractTypingProps(int contractId)
        {
            if (contractId == 0)
            {
                return null;
            }

            return _database?.Contracts?.Find(x => x.Id == contractId)?.Select(s => new
            {
                IsOneOfMultiple = s.IsOneOfMultiple,
                MultipleContractId = s.MultipleContractId,
                IsSubContract = s.IsSubContract,
                SubContractId = s.SubContractId,
                IsAgreementContract = s.IsAgreementContract,
                AgreementContractId = s.AgreementContractId
            })?.FirstOrDefault();
        }

        private void GetChildrenRecursive(int contractId, List<int> childIds, HashSet<int> processedIds)
        {
            if (processedIds.Contains(contractId))
            {
                return;
            }

            processedIds.Add(contractId);

            var contract = _database.Contracts.Find(x => x.SubContractId == contractId || x.AgreementContractId == contractId || x.MultipleContractId == contractId);
            foreach (var item in contract)
            {
                if (item is not null)
                {
                    childIds.Add(item.Id);
                    GetChildrenRecursive(item.Id, childIds, processedIds);
                }
            }
        }
    }
}
