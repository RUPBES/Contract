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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.ServiceCosts.GetById(item.Id) is null)
                {
                    var model = _mapper.Map<ServiceCost>(item);

                    _database.ServiceCosts.Create(model);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create service costs, ID={model.Id}",
                            nameSpace: typeof(ServiceCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return model.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create service costs, object is null",
                            nameSpace: typeof(ServiceCostService).Name,
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
                var model = _database.ServiceCosts.GetById(id);

                if (model is not null)
                {
                    try
                    {
                        _database.ServiceCosts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete service costs, ID={id}",
                            nameSpace: typeof(ServiceCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ServiceCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete service costs, ID is not more than zero",
                            nameSpace: typeof(ServiceCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.ServiceCosts.Update(_mapper.Map<ServiceCost>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update service costs, ID={item.Id}",
                            nameSpace: typeof(ServiceCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update service costs, object is null",
                            nameSpace: typeof(ServiceCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }
    }
}