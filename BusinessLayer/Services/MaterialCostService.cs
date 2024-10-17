using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class MaterialCostService : IMaterialCostService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;

        public MaterialCostService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
           
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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create material costs, ID={model.Id}",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return model.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create material costs, object is null",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

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

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete material costs, ID={id}",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete material costs, ID is not more than zero",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
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

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update material costs, ID={item.Id}",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update material costs, object is null",
                            nameSpace: typeof(MaterialCostService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}