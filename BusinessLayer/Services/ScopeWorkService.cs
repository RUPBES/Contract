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
using System.Diagnostics.Contracts;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;

namespace BusinessLayer.Services
{
    internal class ScopeWorkService : IScopeWorkService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;


        public ScopeWorkService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
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


        /// <summary>
        /// Метод возвращает дату начала и дата окончания объема работ по договору.
        /// Если нет доп.соглашений возвращает с основного объема работ, если есть - возвратит даты с
        /// последнего измененного объема. В случае отсутствия обоих вернет NULL
        /// </summary>  
        /// <param name="contractId"> ID договора, к которому прикреплен объем работ</param>
        /// <returns>tuple(DateTime, DateTime) or Null</returns>
        /// ///
        /// 
        public (DateTime StartDate, DateTime EndDate)? GetScopeWorkPeriodRange(int contractId)
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

            var startPeriod = periodScope?.FirstOrDefault()?.Period ?? new DateTime();
            var endPeriod = periodScope?.LastOrDefault()?.Period ?? new DateTime();

            if (periodScope is null || periodScope.Count() < 1 || startPeriod == default || endPeriod == default)
            {
                return null;
            }

            resultPeriod.start = startPeriod;
            resultPeriod.end = endPeriod;

            return resultPeriod;
        }

        /// <summary>
        /// Возвращает Scope по дате последнего "изменения к договору" или контракту
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="isOwnForces"></param>
        /// <returns></returns>
        public ScopeWorkDTO GetLastScope(int contractId, bool isOwnForces = false)
        {
            var scopeWorks = _database.ScopeWorks
                .Find(a => a.ContractId == contractId && a.IsOwnForces == isOwnForces)
                .Select(scope =>
                {
                    var amendment = _database.ScopeWorkAmendments
                        .Find(s => s.ScopeWorkId == scope.Id)
                        .FirstOrDefault();

                    var amendmentDate = amendment == null ?
                        new DateTime(1900, 1, 1) :
                        _database.Amendments
                            .Find(x => x.Id == amendment.AmendmentId)
                            .Select(x => x.Date)
                            .FirstOrDefault() ?? new DateTime(1900, 1, 1);

                    return (scope, amendmentDate);
                })
                .OrderBy(x => x.amendmentDate)
                .Select(x => x.scope)
                .LastOrDefault();

            return _mapper.Map<ScopeWorkDTO>(scopeWorks);
        }
               

        /*
         * 
         * 
         * Work with Amendments
         * 
         *
         */

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
        public ScopeWorkDTO GetByAmendmentId(int amendmentId)
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
        public void AddAmendment(int amendmentId, int scopeworkId)
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
        public bool? HasNewAmendment(int contractId)
        {
            var amendmentId = _database.Amendments.Find(x => x.ContractId == contractId && x.Type == "scope")?.OrderBy(x => x.Date).LastOrDefault()?.Id;

            if (amendmentId is null)
            {
                return null;
            }

            return _database.ScopeWorkAmendments?.Find(p => p.AmendmentId == amendmentId)?.FirstOrDefault() is not null ? true : false;
        }



        /*     
        *     
        *    
        *    CRUD операции для "родительских" договоров
        *     
        *           
        */

        public bool TryUpdateParentsScopeCosts(ScopeWorkDTO scope, Dictionary<int, ContractType>? parentContracts, CrudOp method, List<SWCostDTO>? previousScope, bool isOneOfMultipleDelete)
        {
            if (scope == null || parentContracts?.Count < 1)
            {
                return false;
            }

            //определяем тип договора, к которому относится объем работ
            var contractType = _database?.Contracts?.Find(x => x.Id == scope.ContractId)
                ?.Select(s => new { s.IsSubContract, s.IsAgreementContract })
                ?.FirstOrDefault();

            bool isSubContracts = contractType?.IsSubContract == true || contractType?.IsAgreementContract == true;

            var operation = (isSubContracts && (method == CrudOp.CREATE || method == CrudOp.UPDATE)) ||
                           (!isSubContracts && method == CrudOp.DELETE)
                ? MathOp.SUBTRACT
                : MathOp.ADD;
            
            if (isOneOfMultipleDelete) // если удаляется подобъект
            {
                foreach (var parentContrId in parentContracts)
                {
                    TryUpdateScopeCosts(scope, operation, parentContrId.Key, isOwnForceUpdate: scope.IsOwnForces.Value, previousScope, isNotCreateNew: true);
                }
            }

            if (!isOneOfMultipleDelete)
            {
                if (!isSubContracts) // для обновления/создания соб.силами саиого договора
                {
                    TryUpdateScopeCosts(scope, operation, scope.ContractId ?? 0, isOwnForceUpdate: true, previousScope);
                }

                foreach (var parentContrId in parentContracts)
                {
                    TryUpdateScopeCosts(scope, operation, parentContrId.Key, isOwnForceUpdate: true, previousScope);

                    //если подобъект, то основные данные по стоимостям тоже обновляем. проверяем, чтобы не обнолялась запись основн
                    if (!isSubContracts)
                    {
                        TryUpdateScopeCosts(scope, operation, parentContrId.Key, isOwnForceUpdate: false, previousScope);
                    }
                }
            }
            return false;
        }


        private void TryUpdateScopeCosts(ScopeWorkDTO scope, MathOp operation, int parentContrId, bool isOwnForceUpdate, List<SWCostDTO>? previousScope = null, bool isNotCreateNew = false)
        {
            if (parentContrId > 0 && scope != null)
            {
                var parentScope = GetLastScope(parentContrId, isOwnForceUpdate);
                int operationSign = operation == MathOp.SUBTRACT ? -1 : 1;

                if (parentScope != null) //обновляем объем работ
                {
                    foreach (var cost in scope.SWCosts)
                    {

                        var existingCost = parentScope != null ?
                            _database.SWCosts.Find(x => x.ScopeWorkId == parentScope?.Id && DateComparer.IsSameYearAndMonth(x.Period, cost.Period))
                            .FirstOrDefault()
                            : null;

                        var previousCost = previousScope?.FirstOrDefault(x => DateComparer.IsSameYearAndMonth(x.Period, cost.Period)) ?? new SWCostDTO();

                        if (existingCost == null && !isNotCreateNew)
                        {
                            var newCost = cost * operationSign;
                            newCost.Id = default;
                            newCost.ScopeWorkId = parentScope.Id;
                            _database.SWCosts.Create(_mapper.Map<SWCost>(newCost));
                        }
                        else
                        {
                            existingCost += _mapper.Map<SWCost>(operationSign * (cost - previousCost));
                            _database.SWCosts.Update(existingCost);
                        }
                    }

                    _database.Save();
                }
                else //добавляем новый объем работ
                {
                    if (isNotCreateNew)
                    {
                        return;
                    }

                    List<SWCostDTO> costs = new List<SWCostDTO>();
                    foreach (var item in scope.SWCosts)
                    {
                        item.Id = default;
                        costs.Add(item * operationSign);
                    }

                    var scopeParent = new ScopeWorkDTO
                    {
                        ContractId = parentContrId,
                        IsOwnForces = isOwnForceUpdate,
                        SWCosts = costs,
                    };

                    Create(scopeParent);
                }
            }
        }



        #region Table of Scopes Info

        public ScopeWorkReportModel GetScopeWorksInfoTable(int contractId, ScopeType type)
        {
            var report = new ScopeWorkReportModel();

            if (type == ScopeType.Both)
            {
                var scopes = GetLastScope(contractId);
                var group = GetGroupTable(scopes, contractId);
                report.Scopes.Add("scope", group);

                var scopesOwn = GetLastScope(contractId, true);
                var groupOwn = GetGroupTable(scopesOwn, contractId);
                report.Scopes.Add("scopeOwn", groupOwn);
            }
            if (type == ScopeType.NoOwn)
            {
                var scopes = GetLastScope(contractId);
                var group = GetGroupTable(scopes, contractId);
                report.Scopes.Add("scope", group);
            }
            if (type == ScopeType.Own)
            {
                var scopesOwn = GetLastScope(contractId, true);
                var groupOwn = GetGroupTable(scopesOwn, contractId);
                report.Scopes.Add("scopeOwn", groupOwn);
            }

            return report;
        }

        private List<ScopeWorkGroup> GetGroupTable(ScopeWorkDTO scopes, int contractId)
        {
            var group = new List<ScopeWorkGroup>();
            var currentYear = DateTime.Now.Year;

            // Создаем группы для разных типов затрат
            (string type, Func<SWCostDTO, decimal?> selector)[] costGroups = {
                (Constants.NDS_TP, (SWCostDTO x) => x.CostNds),
                (Constants.SMR_TP, (SWCostDTO x) => x.SmrCost),
                (Constants.PNR_TP, (SWCostDTO x) => x.PnrCost),
                (Constants.ADD_TP, (SWCostDTO x) => x.AdditionalCost),
                (Constants.EQPT_TP, (SWCostDTO x) => x.EquipmentCost),
                (Constants.OTHER_TP, (SWCostDTO x) => x.OtherExpensesCost + x.GenServiceCost + x.MaterialCost),
                (Constants.NoNDS_TP, (SWCostDTO x) => x.CostNoNds),

                (Constants.MATRL_CLIENT_TP, (SWCostDTO x) => x.MaterialCost),
                (Constants.EQPT_CLIENT_TP, (SWCostDTO x) => x.EquipmentCost),
            };

            // Для каждого типа затрат создаем группу с расчетами
            foreach ((string type, Func<SWCostDTO, decimal?> selector) in costGroups)
            {
                var costs = scopes?.SWCosts ?? new List<SWCostDTO>();
                var selectCosts = costs.Select(x => new Cost
                {
                    Period = x.Period.Value,
                    Value = (type == Constants.EQPT_CLIENT_TP || type == Constants.MATRL_CLIENT_TP) ? 0M : selector(x)
                }).ToList();

                group.Add(new ScopeWorkGroup
                {
                    WorkType = type,
                    Price = costs.Sum(x => selector(x)) ?? 0,
                    Remaining = costs.Where(x => x.Period?.Year >= currentYear).Sum(x => selector(x)) ?? 0,
                    CompletedBeforeYear = costs.Where(x => x.Period?.Year < currentYear).Sum(x => selector(x)) ?? 0,
                    VolumeThisYear = costs.Where(x => x.Period?.Year == currentYear).Sum(x => selector(x)) ?? 0,
                    Costs = selectCosts
                });
            }
            return group;
        }

        #endregion
    }
}