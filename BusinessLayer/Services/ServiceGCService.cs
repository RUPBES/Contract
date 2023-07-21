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
    internal class ServiceGCService : IServiceGCService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public ServiceGCService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(ServiceGCDTO item)
        {
            if (item is not null)
            {
                if (_database.ServiceGCs.GetById(item.Id) is null)
                {
                    var service = _mapper.Map<ServiceGc>(item);

                    _database.ServiceGCs.Create(service);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create service of general contractor, ID={service.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

                    return service.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create service of general contractor, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var service = _database.ServiceGCs.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.ServiceGCs.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete service of general contractor, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete service of general contractor, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<ServiceGCDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ServiceGCDTO>>(_database.ServiceGCs.GetAll());
        }

        public ServiceGCDTO GetById(int id, int? secondId = null)
        {
            var act = _database.ServiceGCs.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<ServiceGCDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(ServiceGCDTO item)
        {
            if (item is not null)
            {
                _database.ServiceGCs.Update(_mapper.Map<ServiceGc>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update service of general contractor, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update service of general contractor, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<ServiceGCDTO> Find(Func<ServiceGc, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ServiceGCDTO>>(_database.ServiceGCs.Find(predicate));
        }
    }
}
