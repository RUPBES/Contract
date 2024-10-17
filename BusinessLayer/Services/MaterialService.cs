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
    internal class MaterialService : IMaterialService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;

        public MaterialService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
        }

        public int? Create(MaterialDTO item)
        {
            if (item is not null)
            {
                if (_database.Materials.GetById(item.Id) is null)
                {
                    var amend = _mapper.Map<MaterialGc>(item);

                    _database.Materials.Create(amend);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create material, ID={amend.Id}",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return amend.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create material, object is null",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var material = _database.Materials.GetById(id);

                if (material is not null)
                {
                    try
                    {
                        _database.Materials.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete material, ID={id}",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete material, ID is not more than zero",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<MaterialDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<MaterialDTO>>(_database.Materials.GetAll());
        }

        public MaterialDTO GetById(int id, int? secondId = null)
        {
            var material = _database.Materials.GetById(id);

            if (material is not null)
            {
                return _mapper.Map<MaterialDTO>(material);
            }
            else
            {
                return null;
            }
        }

        public void Update(MaterialDTO item)
        {
            if (item is not null)
            {
                _database.Materials.Update(_mapper.Map<MaterialGc>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update material, ID={item.Id}",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update material, object is null",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<MaterialDTO> Find(Func<MaterialGc, bool> predicate)
        {
            return _mapper.Map<IEnumerable<MaterialDTO>>(_database.Materials.Find(predicate));
        }

        public void AddAmendmentToMaterial(int amendmentId, int materialId)
        {
            if (materialId > 0 && amendmentId > 0)
            {
                _database.MaterialAmendments.Create(new MaterialAmendment
                {
                    AmendmentId = amendmentId,
                    MaterialId = materialId
                });

                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"add amendment (ID={materialId}) to amendment (ID={amendmentId})",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not add materialAmendment",
                            nameSpace: typeof(MaterialService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
