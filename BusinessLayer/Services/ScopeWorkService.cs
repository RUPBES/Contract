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

namespace BusinessLayer.Services
{
    internal class ScopeWorkService: IScopeWorkService
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
                    _logger.WriteLog(LogLevel.Information, $"create scope works, ID={scopeWorks.Id}", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return scopeWorks.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create scope works, object is null", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
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
                        _logger.WriteLog(LogLevel.Information, $"delete scope works, ID={id}", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete scope works, ID is not more than zero", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            if (item is not null)
            {
                _database.ScopeWorks.Update(_mapper.Map<ScopeWork>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update scope works, ID={item.Id}", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update scope works, object is null", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
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
                _logger.WriteLog(LogLevel.Information, $"add amendment (ID={amendmentId}) to scope work (ID={scopeworkId})", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not add scopeWorkAmendments", typeof(ScopeWorkService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
        public (DateTime, DateTime)? GetDatePeriodLastOrMainScopeWork(int contractId)
        {
            (DateTime start, DateTime end) resultPeriod;

            //проверяем есть измененный объем работы по доп.соглашению (флаг - IsChange = true), если есть выбираем последний объем работы по ДС
            //если нет, находим основной (без ДС) и берем начальную и конечную дату. Если объема работы для договора не существует,
            //перенаправляем запрос на страницу с данным контрактом

            var scopeChangeMain = _database.ScopeWorks
                        .Find(x => x.ContractId == contractId && x.IsOwnForces != true && x.IsChange == true);

            var scopeMain = _database.ScopeWorks
                .Find(x => x.ContractId == contractId && x.IsOwnForces != true && x.IsChange == false);

            //чтобы отфильтровать последние от всех объемов, ищем ID измененного объема работ
            var changescopeId = scopeChangeMain?.LastOrDefault()?.ChangeScopeWorkId;
            var changeMainScopeId = scopeMain?.LastOrDefault()?.ChangeScopeWorkId;

            if (scopeMain is null)
            {
                return null;
            }

            var periodScope = scopeChangeMain?.Count() > 0 ? scopeChangeMain.Where(x=>x.ChangeScopeWorkId == changescopeId) : scopeMain.Where(x => x.ChangeScopeWorkId == changeMainScopeId);

            var startPeriod = periodScope?.FirstOrDefault() == null ? new DateTime() : (DateTime)periodScope?.FirstOrDefault()?.Period;
            var endPeriod = periodScope?.LastOrDefault() == null ? new DateTime() : (DateTime)periodScope?.LastOrDefault()?.Period;

            if (periodScope is null || periodScope.Count() < 1 || startPeriod == default || endPeriod == default)
            {
                return null;
            }

            resultPeriod.start = startPeriod;
            resultPeriod.end = endPeriod;

            return resultPeriod;
        }
    }
}