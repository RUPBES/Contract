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
    internal class SWCostService : ISWCostService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public SWCostService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(SWCostDTO item)
        {
            if (item is not null)
            {
                if (_database.SWCosts.GetById(item.Id) is null)
                {
                    var model = _mapper.Map<SWCost>(item);

                    _database.SWCosts.Create(model);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create scope work costs, ID={model.Id}", typeof(SWCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return model.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create scope work costs, object is null", typeof(SWCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var model = _database.SWCosts.GetById(id);

                if (model is not null)
                {
                    try
                    {
                        _database.SWCosts.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete scope work costs, ID={id}", typeof(SWCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(SWCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete scope work costs, ID is not more than zero", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<SWCostDTO> Find(Func<SWCost, bool> predicate)
        {
            return _mapper.Map<IEnumerable<SWCostDTO>>(_database.SWCosts.Find(predicate));
        }

        public IEnumerable<SWCostDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<SWCostDTO>>(_database.SWCosts.GetAll());
        }

        public SWCostDTO GetById(int id, int? secondId = null)
        {
            var model = _database.SWCosts.GetById(id);

            if (model is not null)
            {
                return _mapper.Map<SWCostDTO>(model);
            }
            else
            {
                return null;
            }
        }

        public void Update(SWCostDTO item)
        {
            if (item is not null)
            {
                _database.SWCosts.Update(_mapper.Map<SWCost>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update scope work costs, ID={item.Id}", typeof(SWCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update scope work costs, object is null", typeof(SWCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            var periodScope = scope?.SWCosts is null? null : scope?.SWCosts.Where(x => x.ScopeWorkId == scopeId);

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

        public SWCost? GetValueScopeWorkByPeriod(int contractId, DateTime? period, Boolean IsOwn = false)
        {
            var scope = _database.ScopeWorks
                .Find(x => x.ContractId == contractId && x.IsChange == true && x.IsOwnForces == IsOwn)
                .LastOrDefault();
            if (scope is null)
            {
                scope = _database.ScopeWorks
                .Find(x => x.ContractId == contractId && x.IsChange != true).FirstOrDefault();
            }
            if (scope is null)
            {
                return null;
            }
            var scopeId = scope?.Id;
            var answer = _database.SWCosts
                .Find(x => x.Period == period && x.ScopeWorkId == scopeId).LastOrDefault();
            while (answer is null && scope != null) 
            {
                scope = scope.ChangeScopeWork;
                if (scope != null)
                {
                    answer = _database.SWCosts
                    .Find(x => x.Period == period && x.ScopeWorkId == scope.Id).LastOrDefault();
                    if (answer != null)
                    {
                        return answer;
                    }
                }
            }
            if (answer != null)
            {
                return answer;
            }
            return null;
        }
    }
}