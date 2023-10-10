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
    internal class ServiceCostService: IServiceCostService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public ServiceCostService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(ServiceCostDTO item)
        {
            if (item is not null)
            {
                if (_database.ServiceCosts.GetById(item.Id) is null)
                {
                    var model = _mapper.Map<ServiceCost>(item);

                    _database.ServiceCosts.Create(model);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create service costs, ID={model.Id}", typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return model.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create service costs, object is null", typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var model = _database.ServiceCosts.GetById(id);

                if (model is not null)
                {
                    try
                    {
                        _database.ServiceCosts.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete service costs, ID={id}", typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete service costs, ID is not more than zero", typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<ServiceCostDTO> Find(Func<ServiceCost, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ServiceCostDTO>>(_database.ServiceCosts.Find(predicate));
        }

        public IEnumerable<ServiceCostDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ServiceCostDTO>>(_database.ServiceCosts.GetAll());
        }

        public ServiceCostDTO GetById(int id, int? secondId = null)
        {
            var model = _database.ServiceCosts.GetById(id);

            if (model is not null)
            {
                return _mapper.Map<ServiceCostDTO>(model);
            }
            else
            {
                return null;
            }
        }

        public void Update(ServiceCostDTO item)
        {
            if (item is not null)
            {
                _database.ServiceCosts.Update(_mapper.Map<ServiceCost>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update service costs, ID={item.Id}", typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update service costs, object is null", typeof(ServiceCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}