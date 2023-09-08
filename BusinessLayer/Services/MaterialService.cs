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
    internal class MaterialService : IMaterialService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public MaterialService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
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
                    _logger.WriteLog(LogLevel.Information, $"create material, ID={amend.Id}", typeof(MaterialService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

                    return amend.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create material, object is null", typeof(MaterialService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

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
                        _logger.WriteLog(LogLevel.Information, $"delete material, ID={id}", typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete material, ID is not more than zero", typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update material, ID={item.Id}", typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update material, object is null", typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"add amendment (ID={materialId}) to amendment (ID={amendmentId})", typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not add materialAmendment", typeof(MaterialService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}
