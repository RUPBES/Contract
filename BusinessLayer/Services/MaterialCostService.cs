using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class MaterialCostService : IMaterialCostService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public MaterialCostService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(MaterialCostDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.MaterialCosts.GetById(item.Id) is null)
                {
                    var model = _mapper.Map<MaterialCost>(item);

                    _database.MaterialCosts.Create(model);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create material costs, ID={model.Id}",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return model.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create material costs, object is null",
                            nameSpace: typeof(MaterialCostService).Name,
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
                var model = _database.MaterialCosts.GetById(id);

                if (model is not null)
                {
                    try
                    {
                        _database.MaterialCosts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete material costs, ID={id}",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete material costs, ID is not more than zero",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<MaterialCostDTO> Find(Func<MaterialCost, bool> predicate)
        {
            return _mapper.Map<IEnumerable<MaterialCostDTO>>(_database.MaterialCosts.Find(predicate));
        }

        public IEnumerable<MaterialCostDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<MaterialCostDTO>>(_database.MaterialCosts.GetAll());
        }

        public MaterialCostDTO GetById(int id, int? secondId = null)
        {
            var model = _database.MaterialCosts.GetById(id);

            if (model is not null)
            {
                return _mapper.Map<MaterialCostDTO>(model);
            }
            else
            {
                return null;
            }
        }

        public void Update(MaterialCostDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.MaterialCosts.Update(_mapper.Map<MaterialCost>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update material costs, ID={item.Id}",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update material costs, object is null",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }
    }
}