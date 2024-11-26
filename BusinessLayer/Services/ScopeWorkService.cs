using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;

namespace BusinessLayer.Services
{
    internal class ScopeWorkService : IScopeWorkService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;


        public ScopeWorkService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(ScopeWorkDTO item)
        {
            if (item is not null)
            {
                if (_database.ScopeWorks.GetById(item.Id) is null)
                {
                    var scopeWorks = _mapper.Map<ScopeWork>(item);

                    _database.ScopeWorks.Create(scopeWorks);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create scope works, ID={scopeWorks.Id}",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return scopeWorks.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create scope works, object is null",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public ScopeWorkDTO GetById(int id, int? secondId = null)
        {
            var scopeWorks = _database.ScopeWorks.GetById(id);

            if (scopeWorks is not null)
            {
                return _mapper.Map<ScopeWorkDTO>(scopeWorks);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ScopeWorkDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ScopeWorkDTO>>(_database.ScopeWorks.GetAll());
        }

        public IEnumerable<ScopeWorkDTO> Find(Func<ScopeWork, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ScopeWorkDTO>>(_database.ScopeWorks.Find(predicate));
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var scopeWorks = _database.ScopeWorks.GetById(id);

                if (scopeWorks is not null)
                {
                    try
                    {
                        _database.ScopeWorks.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete scope works, ID={id}",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete scope works, ID is not more than zero",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public void Update(ScopeWorkDTO item)
        {
            if (item is not null)
            {
                _database.ScopeWorks.Update(_mapper.Map<ScopeWork>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update scope works, ID={item.Id}",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update scope works, object is null",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Метод возвращает дату начала и дата окончания объема работ по договору.
        /// Если нет доп.соглашений возвращает с основного объема работ, если есть - возвратит даты с
        /// последнего измененного объема. В случае отсутствия обоих вернет NULL
        /// </summary>  
        /// <param name="contractId"> ID договора, к которому прикреплен объем работ</param>
        /// <returns>tuple(DateTime, DateTime) or Null</returns>
        /// ///
        /// 
        public (DateTime, DateTime)? GetPeriodRangeScopeWork(int contractId)
        {
            (DateTime start, DateTime end) resultPeriod;

            //проверяем есть измененный объем работы по доп.соглашению (флаг - IsChange = true), если есть выбираем последний объем работы по ДС
            //если нет, находим основной (без ДС) и берем начальную и конечную дату. Если объема работы для договора не существует,
            //возвращаем NULL

            var scope = _database.ScopeWorks
                .Find(x => x.ContractId == contractId && x.IsChange == true)
                .LastOrDefault();

            //если объем по измененным равен NULL смотрим основной(без изменений)
            if (scope is null)
            {
                scope = _database.ScopeWorks
                .Find(x => x.ContractId == contractId && x.IsChange != true).FirstOrDefault();
            }

            //если и основной объем равен NULL возвращаем NULL
            if (scope is null)
            {
                return null;
            }

            //чтобы найти стоимость по смр, пнр и т.д. всех объемов, ищем ID измененного(если нету - основного) объема работ
            var scopeId = scope?.Id;
            var periodScope = scope?.SWCosts is null ? null : scope?.SWCosts.Where(x => x.ScopeWorkId == scopeId);

            var startPeriod = periodScope?.FirstOrDefault() == null ? new DateTime() : (DateTime)periodScope.FirstOrDefault().Period;
            var endPeriod = periodScope?.LastOrDefault() == null ? new DateTime() : (DateTime)periodScope.LastOrDefault().Period;

            if (periodScope is null || periodScope.Count() < 1 || startPeriod == default || endPeriod == default)
            {
                return null;
            }

            resultPeriod.start = startPeriod;
            resultPeriod.end = endPeriod;

            return resultPeriod;
        }

        public void AddAmendmentToScopeWork(int amendmentId, int scopeworkId)
        {
            if (amendmentId > 0 && scopeworkId > 0)
            {
                _database.ScopeWorkAmendments.Create(new ScopeWorkAmendment
                {
                    AmendmentId = amendmentId,
                    ScopeWorkId = scopeworkId
                });

                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"add amendment (ID={amendmentId}) to scope work (ID={scopeworkId})",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not add scopeWorkAmendments",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public AmendmentDTO? GetAmendmentByScopeId(int scopeId)
        {
            try
            {
                var amendId = _database.ScopeWorkAmendments?.Find(p => p.ScopeWorkId == scopeId)?.Select(x => x.AmendmentId).FirstOrDefault();
                if (amendId == 0) { return null; }
                var amend = _database.Amendments.GetById((int)amendId);
                return _mapper.Map<AmendmentDTO>(amend);
            }
            catch (Exception ex) { return null; }
        }

        public IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId)
        {
            var list = _database.Amendments.Find(a => a.ContractId == contractId).ToList();
            List<Amendment> answer = new List<Amendment>();
            foreach (var item in list)
            {
                var ob = _database.ScopeWorkAmendments.Find(s => s.AmendmentId == item.Id).FirstOrDefault();
                if (ob == null)
                    answer.Add(item);
            }
            return _mapper.Map<IEnumerable<AmendmentDTO>>(answer);
        }

        public ScopeWorkDTO GetScopeByAmendment(int amendmentId)
        {
            if (amendmentId != 0)
            {
                var scopeId = _database.ScopeWorkAmendments.Find(a => a.AmendmentId == amendmentId).Select(a => a.ScopeWorkId).FirstOrDefault();
                if (scopeId != null && scopeId != 0)
                {
                    return _mapper.Map<ScopeWorkDTO>(_database.ScopeWorks.GetById(scopeId)); ;
                }
                return null;
            }
            else return null;
        }

        /// <summary>
        /// Возвращает Scope по дате последнего "изменения к договору" или контракту
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="isOwnForces"></param>
        /// <returns></returns>
        public ScopeWorkDTO GetLastScope(int contractId, bool isOwnForces = false)
        {
            var list = _database.ScopeWorks.Find(a => a.ContractId == contractId && a.IsOwnForces == isOwnForces).ToList();
            List<(ScopeWork, DateTime)> listSort = new List<(ScopeWork, DateTime)>();
            foreach (var item in list)
            {
                (ScopeWork, DateTime) obj;
                var ob = _database.ScopeWorkAmendments.Find(s => s.ScopeWorkId == item.Id).FirstOrDefault();
                if (ob == null)
                    obj.Item2 = new DateTime(1900, 1, 1);
                else obj.Item2 = (DateTime)_database.Amendments.Find(x => x.Id == ob.AmendmentId).Select(x => x.Date).FirstOrDefault();
                obj.Item1 = item;
                listSort.Add(obj);
            }
            listSort = listSort.OrderBy(x => x.Item2).ToList();
            var answer = _mapper.Map<ScopeWork>(listSort.Select(x => x.Item1).LastOrDefault());
            return _mapper.Map<ScopeWorkDTO>(answer);
        }

        public AmendmentDTO GetLastAmendmentWithScope(int contractId)
        {
            var list = _database.ScopeWorks.Find(a => a.ContractId == contractId && a.IsOwnForces == false).ToList();
            List<(ScopeWork, Amendment)> listSort = new List<(ScopeWork, Amendment)>();
            foreach (var item in list)
            {
                (ScopeWork, Amendment) obj;
                var ob = _database.ScopeWorkAmendments.Find(s => s.ScopeWorkId == item.Id).FirstOrDefault();
                if (ob != null)
                {
                    obj.Item2 = _database.Amendments.Find(x => x.Id == ob.AmendmentId).FirstOrDefault();
                    obj.Item1 = item;
                    listSort.Add(obj);
                }
            }
            listSort = listSort.OrderBy(x => x.Item2.Date).ToList();
            var answer = _mapper.Map<AmendmentDTO>(listSort.Select(x => x.Item2).LastOrDefault());
            return answer;
        }

        /// <summary>
        /// Перерасчет объемов работ вышестоящего контркта
        /// </summary>
        /// <param name="multipleContractId"></param>
        /// <param name="subobjId"></param>
        /// <param name="type"></param>
        public Boolean EditCostMainContract(int mainContractId, int contractId, ContractType type)
        {
            if (mainContractId <= 0) return false;
            if (contractId <= 0) return false;
            if (type == ContractType.MultipleContract)
            {
                var scopeContractId = GetLastScope(contractId)?.Id;                
                if (scopeContractId.HasValue)
                {
                    RemoveCostsFromMain(mainContractId, (int)scopeContractId, true);
                    RemoveCostsFromMain(mainContractId, (int)scopeContractId, false);

                    _database.Save();
                }
                return true;
            }
            else
            {
                var scopeContractId = GetLastScope(contractId)?.Id;
                var costs = _database.SWCosts.Find(x => x.ScopeWorkId == scopeContractId).ToList();
                if (costs.Count == 0) return true;
                var scopeMainContractId = GetLastScope(mainContractId, true)?.Id;
                var mainCosts = _database.SWCosts.Find(x => x.ScopeWorkId == scopeMainContractId).ToList();
                if (mainCosts.Count > 0)
                {                    
                    foreach (var cost in costs)
                    {
                        var costMain = mainCosts.FirstOrDefault(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)cost.Period));
                        if (costMain is not null)
                        {
                            costMain = SubstractCosts(costMain, _mapper.Map<SWCost>(cost), 1);
                            _database.SWCosts.Update(costMain);
                        }
                    }
                }
                else
                {
                    var scopeWork = new ScopeWork()
                    {
                        ContractId = mainContractId,
                        IsOwnForces = true
                    };
                    _database.ScopeWorks.Create(scopeWork);
                    _database.Save();

                    foreach (var item in costs)
                    {
                        _database.SWCosts.Create(
                                new SWCost()
                                {
                                    Period = item.Period,
                                    PnrCost = -1 * item.PnrCost ?? 0,
                                    SmrCost = -1 * item.SmrCost ?? 0,
                                    EquipmentCost = -1 * item.EquipmentCost ?? 0,
                                    OtherExpensesCost = -1 * item.OtherExpensesCost ?? 0,
                                    AdditionalCost = -1 * item.AdditionalCost ?? 0,
                                    GenServiceCost = -1 * item.GenServiceCost ?? 0,
                                    MaterialCost = -1 * item.MaterialCost ?? 0,
                                    IsOwnForces = true,
                                    ScopeWorkId = scopeWork.Id

                                });
                    }
                }
                _database.Save();
                return true;
            }            

            void RemoveCostsFromMain(int parentContractId, int contractScopeId, bool isOwnForces)
            {
                var mainScpId = GetLastScope(parentContractId, isOwnForces)?.Id;

                if (mainScpId.HasValue)
                {
                    foreach (var item in _database.SWCosts.Find(x => x.ScopeWorkId == mainScpId))
                    {
                        var removeCost = _database.SWCosts.Find(x => 
                                    Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)item.Period) && 
                                    x.ScopeWorkId == contractScopeId).LastOrDefault();

                        _database.SWCosts.Update(SubstractCosts(item, removeCost, -1));
                    }
                }
            }

            SWCost SubstractCosts(SWCost firstCosts, SWCost secondCosts, int opr)
            {
                firstCosts.PnrCost = firstCosts.PnrCost + opr * (secondCosts?.PnrCost ?? 0);
                firstCosts.SmrCost = firstCosts.SmrCost + opr * (secondCosts?.SmrCost ?? 0);
                firstCosts.EquipmentCost = firstCosts.EquipmentCost + opr * (secondCosts?.EquipmentCost ?? 0);
                firstCosts.OtherExpensesCost = firstCosts.OtherExpensesCost + opr * (secondCosts?.OtherExpensesCost ?? 0);
                firstCosts.AdditionalCost = firstCosts.AdditionalCost + opr * (secondCosts?.AdditionalCost ?? 0);
                firstCosts.GenServiceCost = firstCosts.GenServiceCost + opr * (secondCosts?.GenServiceCost ?? 0);
                firstCosts.MaterialCost = firstCosts.MaterialCost + opr * (secondCosts?.MaterialCost ?? 0);

                return firstCosts;
            }
        }

        /*new, for all methods*/
        public void UpdateParentCosts(int parentContrId, List<SWCostDTO> costs, bool isOwnForces, int operatorSign, int? changeScopeId = null)
        {
            if (parentContrId != 0)
            {
                var parentScpId = _database.ScopeWorks.Find(x => x.ContractId == parentContrId && x.IsOwnForces == isOwnForces).FirstOrDefault()?.Id;

                /******  По ID объема работ, который изменяется по доп.соглашению, берем старые данные объемов   */
                var oldChaildCosts = changeScopeId.HasValue && changeScopeId > 0 ? _database.SWCosts.Find(x => x.ScopeWorkId == changeScopeId) : null;

                if (parentScpId.HasValue && parentScpId > 0)
                {
                    foreach (var newChaildCosts in costs)
                    {
                        var parentCosts = _mapper.Map<SWCostDTO>( _database.SWCosts.Find(x => x.ScopeWorkId == parentScpId &&
                                                                x.Period?.Year == newChaildCosts.Period?.Year &&
                                                                x.Period?.Month == newChaildCosts.Period?.Month).FirstOrDefault());

                        var oldChaildCost = oldChaildCosts?.FirstOrDefault(x =>
                                                   x.Period?.Year == newChaildCosts.Period?.Year &&
                                                   x.Period?.Month == newChaildCosts.Period?.Month);

                        if (parentCosts is not null)   /****** обновляем стоимость конкретного периода родительского объема работ   */
                        {
                            parentCosts = SubstractOldAndAddNewCosts(parentCosts, _mapper.Map<SWCostDTO>(oldChaildCost), newChaildCosts, operatorSign);
                            _database.SWCosts.Update(_mapper.Map<SWCost>(parentCosts));
                        }
                        else
                        {
                            CreateCostsByScopeId((int)parentScpId, _mapper.Map<SWCost>(newChaildCosts), isOwnForces); /*  добавление стоимости за конкретный период   */
                        }
                    }
                    _database.Save();
                }
                else
                {
                    //   создаем новый и добавляем стоимости для него!!!!!!!!!!!!!!!!!!!!
                    if (isOwnForces)
                    CreateOwnScopeFromSubandAgrContracts(parentContrId, _mapper.Map<List<SWCost>>(costs));
                    else
                    {
                        var scope = new ScopeWork();
                        scope.ContractId = parentContrId;
                        scope.IsOwnForces = false;
                        foreach (var item in _mapper.Map<List<SWCost>>(costs))
                        {
                            var swCost = new SWCost();
                            swCost = SubstractCosts(swCost, item, 1);
                            swCost.Period = item.Period;
                            swCost.ScopeWork = scope;
                            scope.SWCosts.Add(swCost);
                        }
                        _database.ScopeWorks.Create(scope);
                        foreach (var item in scope.SWCosts)
                            _database.SWCosts.Create(item);
                        _database.Save();
                    }
                }
            }

            void CreateOwnScopeFromSubandAgrContracts(int id, List<SWCost> costs)
            {
                var subContracts = _database.Contracts.Find(x => x.SubContractId == id).ToList();
                var agrContracts = _database.Contracts.Find(x => x.AgreementContractId == id).ToList();
                var scope = new ScopeWork();
                scope.ContractId = id;
                scope.IsOwnForces = true;
                foreach (var item in subContracts)
                {
                    var subScope = GetLastScope(item.Id);
                    foreach (var swcost in _database.SWCosts.Find(x => x.ScopeWorkId == subScope.Id).ToList())
                    {
                        var swmain = scope.SWCosts.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)swcost.Period)).FirstOrDefault();
                        if (swmain != null)
                        {
                            swmain = SubstractCosts(swmain, swcost, -1);
                        }
                        else
                        {
                            var sw = new SWCost();
                            sw.ScopeWork = scope;
                            sw.Period = swcost.Period;
                            sw = SubstractCosts(sw, swcost, -1);
                            scope.SWCosts.Add(sw);
                        }
                    }
                }
                foreach (var item in agrContracts)
                {
                    var subScope = GetLastScope(item.Id);
                    foreach (var swcost in _database.SWCosts.Find(x => x.ScopeWorkId == subScope.Id).ToList())
                    {
                        var swmain = scope.SWCosts.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)swcost.Period)).FirstOrDefault();
                        if (swmain != null)
                        {
                            swmain = SubstractCosts(swmain, swcost, -1);
                        }
                        else
                        {
                            var sw = new SWCost();
                            sw.ScopeWork = scope;
                            sw.Period = swcost.Period;
                            sw = SubstractCosts(sw, swcost, -1);
                            scope.SWCosts.Add(sw);
                        }
                    }
                }
                foreach (var item in costs)
                {
                    var swmain = scope.SWCosts.Where(x => Checker.EquallyDateByMonth((DateTime)x.Period, (DateTime)item.Period)).FirstOrDefault();
                    if (swmain != null)
                    {
                        swmain = SubstractCosts(swmain, item, 1);
                    }
                    else
                    {
                        var sw = new SWCost();
                        sw.ScopeWork = scope;
                        sw.Period = item.Period;
                        sw = SubstractCosts(sw, item, 1);
                        scope.SWCosts.Add(sw);
                    }                    
                }
                _database.ScopeWorks.Create(scope);
                foreach (var item in scope.SWCosts)
                    _database.SWCosts.Create(item);
                _database.Save();
            }

            SWCost SubstractCosts(SWCost firstCosts, SWCost secondCosts, int opr)
            {
                firstCosts.PnrCost = firstCosts.PnrCost + opr * (secondCosts?.PnrCost ?? 0);
                firstCosts.SmrCost = firstCosts.SmrCost + opr * (secondCosts?.SmrCost ?? 0);
                firstCosts.EquipmentCost = firstCosts.EquipmentCost + opr * (secondCosts?.EquipmentCost ?? 0);
                firstCosts.OtherExpensesCost = firstCosts.OtherExpensesCost + opr * (secondCosts?.OtherExpensesCost ?? 0);
                firstCosts.AdditionalCost = firstCosts.AdditionalCost + opr * (secondCosts?.AdditionalCost ?? 0);
                firstCosts.GenServiceCost = firstCosts.GenServiceCost + opr * (secondCosts?.GenServiceCost ?? 0);
                firstCosts.MaterialCost = firstCosts.MaterialCost + opr * (secondCosts?.MaterialCost ?? 0);

                return firstCosts;
            }
        }

        public void AddOwnForcesCostsByScopeId(ScopeWorkDTO scopeWork, int operatorSign = 1)
        {
            var scope = _database.ScopeWorks.Find(x => x.ContractId == scopeWork.ContractId && x.IsOwnForces == true).LastOrDefault();

            if (scope is null)
            {
                scopeWork.IsOwnForces = true;
                _database.ScopeWorks.Create(_mapper.Map<ScopeWork>(scopeWork));
                _database?.Save();
            }
            else
            {
                var existCosts = _database?.SWCosts?.Find(x => x.ScopeWorkId == scope.Id);

                foreach (var newCosts in scopeWork.SWCosts)
                {
                    var existCost = _mapper.Map<SWCostDTO>(existCosts?.FirstOrDefault(x => x.Period?.Year == newCosts.Period?.Year && x.Period?.Month == newCosts.Period?.Month));

                    if (existCost is not null)   /*****  обновляем стоимость конкретного периода родительского объема работ      */
                    {
                        existCost = SubstractOldAndAddNewCosts(existCost, new SWCostDTO(), newCosts, operatorSign);
                        _database?.SWCosts.Update(_mapper.Map<SWCost>(existCost));
                    }
                    else
                    {
                        CreateCostsByScopeId(scope.Id, _mapper.Map<SWCost>(newCosts), true); /****  добавление стоимости за конкретный период   */
                    }
                }
                _database?.Save();
            }
        }

        #region AdditionsMethods
        public void RemoveSubContractCost(int costId, int contractId, Dictionary<int, ContractType> parentContracts, int operatorSign = -1)
        {
            var cost = _database.SWCosts.GetById(costId);
            var costRemove = new List<SWCostDTO>();
            costRemove.Add(_mapper.Map<SWCostDTO>(cost));
            var ownScope = _database.ScopeWorks.Find(x => x.ContractId == contractId && x.IsOwnForces == true).LastOrDefault();

            //если есть собственные силы по данному периоду, удаляем
            if (ownScope is not null)
            {
                var ownCost = _database.SWCosts.Find(x => x.ScopeWorkId == ownScope.Id && x.Period?.Year == cost?.Period?.Year && x.Period?.Month == cost?.Period?.Month).FirstOrDefault();

                if (ownCost != null && !IsHaveChildContracts(contractId))
                {
                    _database.SWCosts.Delete(ownCost.Id);
                }
                else
                {
                    if (parentContracts[contractId] != ContractType.GenСontract)
                    {
                        UpdateParentCosts(contractId, costRemove, true, operatorSign);
                    }
                }
            }

            //обновляем родительский договор

            foreach (var item in parentContracts)
            {
                UpdateParentCosts(item.Key, costRemove, true, operatorSign);
            }

            bool IsHaveChildContracts(int contractId)
            {
                var childrenContracts = _database.Contracts.Find(x => x.AgreementContractId == contractId || x.SubContractId == contractId || x.MultipleContractId == contractId).Count();
                return childrenContracts > 0 ? true : false;
            }
        }       

        private void CreateCostsByScopeId(int scopeId, SWCost cost, bool isOwnForces)
        {
            if (scopeId > 0)
            {
                _database.SWCosts.Create(new SWCost
                {
                    Period = cost.Period,
                    PnrCost = cost.PnrCost,
                    SmrCost = cost.SmrCost,
                    EquipmentCost = cost.EquipmentCost,
                    OtherExpensesCost = cost.OtherExpensesCost,
                    AdditionalCost = cost.AdditionalCost,
                    GenServiceCost = cost.GenServiceCost,
                    MaterialCost = cost.MaterialCost,
                    IsOwnForces = isOwnForces,
                    ScopeWorkId = scopeId,
                });

                _database.Save();
            }
        }   

        private SWCostDTO SubstractOldAndAddNewCosts(SWCostDTO scpMain, SWCostDTO oldSwCost, SWCostDTO scpNew, int opr)
        {
            scpMain.PnrCost = (scpMain.PnrCost ?? 0) + opr * ((scpNew?.PnrCost ?? 0) - (oldSwCost?.PnrCost ?? 0));
            scpMain.SmrCost = (scpMain.SmrCost ?? 0) + opr * ((scpNew?.SmrCost ?? 0) - (oldSwCost?.SmrCost ?? 0));
            scpMain.EquipmentCost = (scpMain.EquipmentCost ?? 0) + opr * ((scpNew?.EquipmentCost ?? 0) - (oldSwCost?.EquipmentCost ?? 0));
            scpMain.OtherExpensesCost = (scpMain.OtherExpensesCost ?? 0) + opr * ((scpNew?.OtherExpensesCost ?? 0) - (oldSwCost?.OtherExpensesCost ?? 0));
            scpMain.AdditionalCost = (scpMain.AdditionalCost ?? 0) + opr * ((scpNew?.AdditionalCost ?? 0) - (oldSwCost?.AdditionalCost ?? 0));
            scpMain.GenServiceCost = (scpMain.GenServiceCost ?? 0) + opr * ((scpNew?.GenServiceCost ?? 0) - (oldSwCost?.GenServiceCost ?? 0));
            scpMain.MaterialCost = (scpMain.MaterialCost ?? 0) + opr * ((scpNew?.MaterialCost ?? 0) - (oldSwCost?.MaterialCost ?? 0));

            return scpMain;
        }     
        #endregion
    }
}