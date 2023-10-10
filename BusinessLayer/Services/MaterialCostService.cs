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
            if (item is not null)
            {
                if (_database.MaterialCosts.GetById(item.Id) is null)
                {
                    var model = _mapper.Map<MaterialCost>(item);

                    _database.MaterialCosts.Create(model);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create material costs, ID={model.Id}", typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return model.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create material costs, object is null", typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var model = _database.MaterialCosts.GetById(id);

                if (model is not null)
                {
                    try
                    {
                        _database.MaterialCosts.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete material costs, ID={id}", typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete material costs, ID is not more than zero", typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            if (item is not null)
            {
                _database.MaterialCosts.Update(_mapper.Map<MaterialCost>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update material costs, ID={item.Id}", typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update material costs, object is null", typeof(MaterialCostService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}