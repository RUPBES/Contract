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
                    _logger.WriteLog(LogLevel.Information, $"create scope works, ID={scopeWorks.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return scopeWorks.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create scope works, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

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
                        _logger.WriteLog(LogLevel.Information, $"delete scope works, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete scope works, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update scope works, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update scope works, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"add amendment (ID={amendmentId}) to scope work (ID={scopeworkId})", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not add scopeWorkAmendments", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}