﻿using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return scopeWorks.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create scope works, object is null",
                            nameSpace: typeof(ScopeWorkService).Name,
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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete scope works, ID is not more than zero",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<ScopeWorkDTO> Find(Func<ScopeWork, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ScopeWorkDTO>>(_database.ScopeWorks.Find(predicate));
        }

        public IEnumerable<ScopeWorkDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ScopeWorkDTO>>(_database.ScopeWorks.GetAll());
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

        public void Update(ScopeWorkDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.ScopeWorks.Update(_mapper.Map<ScopeWork>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update scope works, ID={item.Id}",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update scope works, object is null",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public void AddAmendmentToScopeWork(int amendmentId, int scopeworkId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not add scopeWorkAmendments",
                            nameSpace: typeof(ScopeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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

        public AmendmentDTO? GetAmendmentByScopeId(int scopeId)
        {
            try
            {
                var amendId = _database.ScopeWorkAmendments?.Find(p => p.ScopeWorkId == scopeId)?.FirstOrDefault().AmendmentId;
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

        public ScopeWork GetLastScope(int contractId, bool isOwnForces = false)
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
            return _mapper.Map<ScopeWork>(listSort.Select(x => x.Item1).LastOrDefault());
        }



        public void AddSWCostForMainContract(int? scopeId, List<SWCostDTO> costs)
        {
            if (scopeId is not null && scopeId > 0)
            {
                var swCosts = _database.SWCosts.Find(x => x.ScopeWorkId == scopeId);

                foreach (var item in costs)
                {
                    var oneSwCost = swCosts.FirstOrDefault(x => x.Period?.Year == item.Period?.Year && x.Period?.Month == item.Period?.Month);
                    if (oneSwCost is not null)
                    {
                        oneSwCost.PnrCost += item.PnrCost ?? 0;
                        oneSwCost.SmrCost += item.SmrCost ?? 0;
                        oneSwCost.EquipmentCost += item.EquipmentCost ?? 0;
                        oneSwCost.OtherExpensesCost += item.OtherExpensesCost ?? 0;
                        oneSwCost.AdditionalCost += item.AdditionalCost ?? 0;
                        oneSwCost.GenServiceCost += item.GenServiceCost ?? 0;
                        oneSwCost.MaterialCost += item.MaterialCost ?? 0;

                        _database.SWCosts.Update(oneSwCost);
                    }
                    else
                    {
                        _database.SWCosts.Create(new SWCost
                        {
                            Period = item.Period,
                            PnrCost = item.PnrCost ?? 0,
                            SmrCost = item.SmrCost ?? 0,
                            EquipmentCost = item.EquipmentCost ?? 0,
                            OtherExpensesCost = item.OtherExpensesCost ?? 0,
                            AdditionalCost = item.AdditionalCost ?? 0,
                            GenServiceCost = item.GenServiceCost ?? 0,
                            MaterialCost = item.MaterialCost ?? 0,
                            IsOwnForces = item.IsOwnForces,
                            ScopeWorkId = scopeId,
                        });
                    }
                }
                _database.Save();
            }
        }

        public void CreateSWCostForMainContract(int? scopeId, List<SWCostDTO> costs, bool isOwnForces)
        {
            if (scopeId is not null && scopeId > 0)
            {
                foreach (var item in costs)
                {
                    _database.SWCosts.Create(new SWCost
                    {
                        Period = item.Period,
                        PnrCost = item.PnrCost,
                        SmrCost = item.SmrCost,
                        EquipmentCost = item.EquipmentCost,
                        OtherExpensesCost = item.OtherExpensesCost,
                        AdditionalCost = item.AdditionalCost,
                        GenServiceCost = item.GenServiceCost,
                        MaterialCost = item.MaterialCost,
                        IsOwnForces = isOwnForces,
                        ScopeWorkId = scopeId,
                    });
                }
                _database.Save();
            }
        }


        /// <summary>
        /// для добавления и удаления объемов соб.силами главного контракта, из суб-да или соглашения с ф-лом
        /// </summary>
        /// <param name="mainOwnContrId">ID главного контракта</param>
        /// <param name="cost">объемы для вычета или добавления</param>
        /// <param name="addOrSubstr"> -1 - вычесть, 1 - добавить</param>
        public void AddOrSubstractCostsOwnForceMnContract(int? mainOwnContrId, List<SWCostDTO> cost, int addOrSubstr)
        {
            if (mainOwnContrId.HasValue && cost.Count > 0)
            {
                var mainOwnScpId = _database.ScopeWorks.Find(x => x.ContractId == mainOwnContrId && x.IsOwnForces == true)
                    .FirstOrDefault()?.Id;
                //находим главного договора объемы соб.силами и меняем их
                var mainScpCosts = mainOwnScpId.HasValue ? _database.SWCosts.Find(x => x.ScopeWorkId == mainOwnScpId) : null;

                if (mainScpCosts is not null)
                {
                    foreach (var item in cost)
                    {
                        var costMain = mainScpCosts.FirstOrDefault(x => x.Period?.Year == item.Period?.Year && x.Period?.Month == item.Period?.Month);
                        if (costMain is not null)
                        {
                            costMain = SubstractCosts(costMain, _mapper.Map<SWCost>(item), addOrSubstr);
                            _database.SWCosts.Update(costMain);
                        }
                    }
                }
                else
                {
                    var scWork = new ScopeWork()
                    {
                        ContractId = mainOwnContrId,
                        IsOwnForces = true
                    };
                    _database.ScopeWorks.Create(scWork);
                    _database.Save();

                    foreach (var item in cost)
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
                                    ScopeWorkId = scWork.Id

                                });
                    }
                }
                _database.Save();
            }
        }

        /// <summary>
        /// Обновление сумм объема работ главного объема (основного договора)
        /// </summary>
        /// <param name="mainContractId">ID главного контракта</param>
        /// <param name="changeScopeId">ID объема работ, который изменяется</param>
        /// <param name="costs"> объем работ, суммы которого изменяют объемы работ гланого контракта</param>
        public void UpdateCostOwnForceMnContract(int? mainContractId, int changeScopeId, List<SWCostDTO> costs, bool isOnePartOfMultiContr = false)
        {
            if (mainContractId > 0 && changeScopeId > 0)
            {
                var costsOld = _database.SWCosts.Find(x => x.ScopeWorkId == changeScopeId);
                var mainOwnScopeId = _database.ScopeWorks.Find(x => x.ContractId == mainContractId && x.IsOwnForces == true)?.FirstOrDefault()?.Id;

                if (mainOwnScopeId.HasValue && costsOld.Count() > 0)
                {
                    int opertr = isOnePartOfMultiContr ? 1 : -1;
                    foreach (var scpNew in costs)
                    {
                        var scpMain = _database.SWCosts
                            .Find(x => x.ScopeWorkId == mainOwnScopeId &&
                                  x.Period?.Year == scpNew.Period?.Year &&
                                  x.Period?.Month == scpNew.Period?.Month)
                            .FirstOrDefault();

                        var oldSwCost = costsOld.FirstOrDefault(x => x.Period?.Year == scpNew.Period?.Year &&
                                  x.Period?.Month == scpNew.Period?.Month);

                        if (scpMain is not null)
                        {
                            scpMain = SubstractOldAndAddNewCosts(scpMain, oldSwCost, _mapper.Map<SWCost>(scpNew), opertr);
                            _database.SWCosts.Update(scpMain);
                        }
                    }
                    _database.Save();
                }

                if (isOnePartOfMultiContr)
                {
                    var mainScopeId = _database.ScopeWorks.Find(x => x.ContractId == mainContractId && x.IsOwnForces == false)?.FirstOrDefault()?.Id;
                    if (mainScopeId.HasValue && costsOld.Count() > 0)
                    {

                        foreach (var scpNew in costs)
                        {
                            var scpMain = _database.SWCosts
                                .Find(x => x.ScopeWorkId == mainScopeId &&
                                      x.Period?.Year == scpNew.Period?.Year &&
                                      x.Period?.Month == scpNew.Period?.Month)
                                .FirstOrDefault();

                            var oldSwCost = costsOld.FirstOrDefault(x => x.Period?.Year == scpNew.Period?.Year &&
                                      x.Period?.Month == scpNew.Period?.Month);

                            if (scpMain is not null)
                            {
                                scpMain = SubstractOldAndAddNewCosts(scpMain, oldSwCost, _mapper.Map<SWCost>(scpNew), 1);
                                _database.SWCosts.Update(scpMain);
                            }
                        }
                        _database.Save();
                    }
                }
            }
        }


        /// <summary>
        /// Удаление одной стоимости объема работ подобъекта из главного объема (основного договора)
        /// </summary>       
        /// <param name="cost"> стоимость одного периода объем работ, по который удаляем из объем работ из гланого договора</param>
        public void RemoveOneCostOfMainContract(int? mainContractScopeId, SWCostDTO cost)
        {
            if (mainContractScopeId.HasValue && mainContractScopeId > 0)
            {
                var scpMain = _database.SWCosts.Find(x => x.ScopeWorkId == mainContractScopeId &&
                x.Period?.Year == cost.Period?.Year && x.Period?.Month == cost.Period?.Month)
                    .FirstOrDefault();

                if (scpMain is not null)
                {
                    scpMain.PnrCost = scpMain.PnrCost - (cost?.PnrCost ?? 0);
                    scpMain.SmrCost = scpMain.SmrCost - (cost?.SmrCost ?? 0);
                    scpMain.EquipmentCost = scpMain.EquipmentCost - (cost?.EquipmentCost ?? 0);
                    scpMain.OtherExpensesCost = scpMain.OtherExpensesCost - (cost?.OtherExpensesCost ?? 0);
                    scpMain.AdditionalCost = scpMain.AdditionalCost - (cost?.AdditionalCost ?? 0);
                    scpMain.GenServiceCost = scpMain.GenServiceCost - (cost?.GenServiceCost ?? 0);
                    scpMain.MaterialCost = scpMain.MaterialCost - (cost?.MaterialCost ?? 0);

                    _database.SWCosts.Update(scpMain);
                }

                _database.Save();
            }
        }

        /// <summary>
        /// Удаление сумм объема работ подобъекта из главного объема (основного договора)
        /// </summary>
        /// <param name="scopeId">ID объема работ</param>       
        public void RemoveCostsOfMainContract(int multipleContractId, int subobjId)
        {
            //1.Удаляем все isOwnForces = true и isOwnForces = false, и вычетаем из основного isOwnForces = true и isOwnForces = false
            //2. удаляем у основного значения подобъекта (последние значения(они явл. текущими)). Оставшиеся просто удаляем!

            if (multipleContractId > 0 && subobjId > 0)
            {
                var subScpId = _database.ScopeWorks.Find(x => x.ContractId == subobjId).LastOrDefault()?.Id;
                if (subScpId.HasValue)
                {
                    RemoveCosts(multipleContractId, (int)subScpId, true);
                    RemoveCosts(multipleContractId, (int)subScpId, false);

                    _database.Save();
                }
            }
        }
       
        public ScopeWork GetScopeByAmendment(int amendmentId)
        {
            if (amendmentId != 0)
            {
                var scopeId = _database.ScopeWorkAmendments.Find(a => a.AmendmentId == amendmentId).Select(a => a.ScopeWorkId).FirstOrDefault();
                if (scopeId != null && scopeId != 0)
                {
                    return _database.ScopeWorks.GetById(scopeId);
                }
                return null;
            }
            else return null;
        } 
        
        #region AdditionsMethods
        private void RemoveCosts(int multipleContractId, int subScpId, bool isOwnForces)
        {
            var mainScpId = _database.ScopeWorks
                .Find(x => x.ContractId == multipleContractId && x.IsOwnForces == isOwnForces)?.LastOrDefault()?.Id;

            if (mainScpId.HasValue)
            {
                foreach (var item in _database.SWCosts.Find(x => x.ScopeWorkId == mainScpId))
                {
                    var removeCost = _database.SWCosts.Find(x => x.Period?.Year == item.Period?.Year &&
                              x.Period?.Month == item.Period?.Month && x.ScopeWorkId == subScpId).LastOrDefault();

                    _database.SWCosts.Update(SubstractCosts(item, removeCost, -1));
                }
            }
        }

        private SWCost SubstractCosts(SWCost firstCosts, SWCost secondCosts, int opr)
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

        private SWCost SubstractOldAndAddNewCosts(SWCost scpMain, SWCost oldSwCost, SWCost scpNew, int opr)
        {
            scpMain.PnrCost = scpMain.PnrCost + opr * ((scpNew?.PnrCost ?? 0) - (oldSwCost?.PnrCost ?? 0));
            scpMain.SmrCost = scpMain.SmrCost + opr * ((scpNew?.SmrCost ?? 0) - (oldSwCost?.SmrCost ?? 0));
            scpMain.EquipmentCost = scpMain.EquipmentCost + opr * ((scpNew?.EquipmentCost ?? 0) - (oldSwCost?.EquipmentCost ?? 0));
            scpMain.OtherExpensesCost = scpMain.OtherExpensesCost + opr * ((scpNew?.OtherExpensesCost ?? 0) - (oldSwCost?.OtherExpensesCost ?? 0));
            scpMain.AdditionalCost = scpMain.AdditionalCost + opr * ((scpNew?.AdditionalCost ?? 0) - (oldSwCost?.AdditionalCost ?? 0));
            scpMain.GenServiceCost = scpMain.GenServiceCost + opr * ((scpNew?.GenServiceCost ?? 0) - (oldSwCost?.GenServiceCost ?? 0));
            scpMain.MaterialCost = scpMain.MaterialCost + opr * ((scpNew?.MaterialCost ?? 0) - (oldSwCost?.MaterialCost ?? 0));

            return scpMain;
        }
        #endregion
    }
}