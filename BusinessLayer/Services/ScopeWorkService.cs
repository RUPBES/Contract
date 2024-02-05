using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

        //TODO: надо удалить?
        public (DateTime, DateTime)? GetFullPeriodRangeScopeWork(int contractId)
        {
            (DateTime start, DateTime end) resultPeriod;

            //проверяем есть измененный объем работы по доп.соглашению (флаг - IsChange = true), если есть выбираем последний объем работы по ДС
            //если нет, находим основной (без ДС) и берем начальную и конечную дату. Если объема работы для договора не существует,
            //возвращаем NULL

            var scope = _database.ScopeWorks
                .Find(x => x.ContractId == contractId && x.IsOwnForces == false).ToList();

            //если и основной объем равен NULL возвращаем NULL
            if (scope.Count == 0 || scope is null)
            {
                return null;
            }

            resultPeriod.start = new DateTime(4000, 1, 1);
            resultPeriod.end = new DateTime(1000, 1, 1);
            foreach (var item in scope)
            {
                foreach (var item2 in item.SWCosts)
                {
                    resultPeriod.start = resultPeriod.start > item2.Period ? (DateTime)item2.Period : resultPeriod.start;
                    resultPeriod.end = resultPeriod.end < item2.Period ? (DateTime)item2.Period : resultPeriod.end;
                }
            }

            if (resultPeriod.end == new DateTime(1000, 1, 1) || resultPeriod.start == new DateTime(4000, 1, 1))
                return null;
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
        /// Удаление одной стоимости объема работ подобъекта из главного объема (основного договора)
        /// </summary>       
        /// <param name="cost"> стоимость одного периода объем работ, по который удаляем из объем работ из гланого договора</param>
        public void SubstractCostFromMainContract(int? mainContractScopeId, SWCostDTO cost)
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
        /// Вычитание сумм объема работ подобъекта из главного объема (основного договора)
        /// </summary>
        /// <param name="scopeId">ID объема работ</param>
        /// <param name="costs"> объем работ, по которому удаляем объемы работ подобъекта и эти же значения вычитаем из гланого объема</param>
        public void SubstractSWCostForMainContract(int? mainContractScopeId, int changeScopeId, List<SWCostDTO> costs)
        {
            if (mainContractScopeId > 0 && changeScopeId > 0)
            {
                var costsOld = _database.SWCosts.Find(x => x.ScopeWorkId == changeScopeId);

                foreach (var scpNew in costs)
                {
                    var scpMain = _database.SWCosts
                        .Find(x => x.ScopeWorkId == mainContractScopeId &&
                              x.Period?.Year == scpNew.Period?.Year &&
                              x.Period?.Month == scpNew.Period?.Month)
                        .FirstOrDefault();

                    var oldSwCost = costsOld
                        .FirstOrDefault(x => x.Period?.Year == scpNew.Period?.Year &&
                              x.Period?.Month == scpNew.Period?.Month);

                    if (scpMain is not null)
                    {
                        scpMain.PnrCost = scpMain.PnrCost + (scpNew?.PnrCost ?? 0) - (oldSwCost?.PnrCost ?? 0);
                        scpMain.SmrCost = scpMain.SmrCost + (scpNew?.SmrCost ?? 0) - (oldSwCost?.SmrCost ?? 0);
                        scpMain.EquipmentCost = scpMain.EquipmentCost + (scpNew?.EquipmentCost ?? 0) - (oldSwCost?.EquipmentCost ?? 0);
                        scpMain.OtherExpensesCost = scpMain.OtherExpensesCost + (scpNew?.OtherExpensesCost ?? 0) - (oldSwCost?.OtherExpensesCost ?? 0);
                        scpMain.AdditionalCost = scpMain.AdditionalCost + (scpNew?.AdditionalCost ?? 0) - (oldSwCost?.AdditionalCost ?? 0);
                        scpMain.GenServiceCost = scpMain.GenServiceCost + (scpNew?.GenServiceCost ?? 0) - (oldSwCost?.GenServiceCost ?? 0);
                        scpMain.MaterialCost = scpMain.MaterialCost + (scpNew?.MaterialCost ?? 0) - (oldSwCost?.MaterialCost ?? 0);

                        _database.SWCosts.Update(scpMain);
                    }
                }
                _database.Save();
            }
        }


        /// <summary>
        /// Удаление сумм объема работ подобъекта из главного объема (основного договора)
        /// </summary>
        /// <param name="scopeId">ID объема работ</param>       
        public void RemoveSWCostFromMainContract(int multipleContractId, int subobjId)
        {
            //1.Удаляем все isOwnForces = true и isOwnForces = false, и вычетаем из основного isOwnForces = true и isOwnForces = false
            //2. удаляем у основного значения подобъекта (последние значения(они явл. текущими)). Оставшиеся просто удаляем!

            if (multipleContractId > 0 && subobjId > 0)
            {
                RemoveFromMainContractCostsOfSub(multipleContractId, subobjId, true);
                RemoveFromMainContractCostsOfSub(multipleContractId, subobjId, false);

                _database.Save();
            }
        }

        private void RemoveFromMainContractCostsOfSub(int multipleContractId, int subobjId, bool isOwnForces)
        {
            var mainScpId = _database.ScopeWorks
                .Find(x => x.ContractId == multipleContractId && x.IsOwnForces == isOwnForces)?.LastOrDefault()?.Id;

            var subScpId = _database.ScopeWorks
                .Find(x => x.ContractId == subobjId && x.IsOwnForces == isOwnForces)?.LastOrDefault()?.Id;
            
            if (mainScpId.HasValue && subScpId.HasValue)
            {
                foreach (var item in _database.SWCosts.Find(x => x.ScopeWorkId == mainScpId))
                {
                    var removeCost = _database.SWCosts
                        .Find(x => x.Period?.Year == item.Period?.Year &&
                              x.Period?.Month == item.Period?.Month &&
                              x.ScopeWorkId == subScpId)
                        .LastOrDefault();

                    item.PnrCost = item.PnrCost - (removeCost?.PnrCost ?? 0);
                    item.SmrCost = item.SmrCost - (removeCost?.SmrCost ?? 0);
                    item.EquipmentCost = item.EquipmentCost - (removeCost?.EquipmentCost ?? 0);
                    item.OtherExpensesCost = item.OtherExpensesCost - (removeCost?.OtherExpensesCost ?? 0);
                    item.AdditionalCost = item.AdditionalCost - (removeCost?.AdditionalCost ?? 0);
                    item.GenServiceCost = item.GenServiceCost - (removeCost?.GenServiceCost ?? 0);
                    item.MaterialCost = item.MaterialCost - (removeCost?.MaterialCost ?? 0);

                    _database.SWCosts.Update(item);
                }
            }
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

        public ScopeWork GetLastScope(int contractId)
        {
            var list = _database.ScopeWorks.Find(a => a.ContractId == contractId).ToList();
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
    }
}