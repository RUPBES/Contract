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
    internal class TypeWorkService : ITypeWorkService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public TypeWorkService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(TypeWorkDTO item)
        {
            if (item is not null)
            {
                if (_database.TypeWorks.GetById(item.Id) is null)
                {
                    var typeWork = _mapper.Map<TypeWork>(item);

                    _database.TypeWorks.Create(typeWork);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create a type of work, ID={typeWork.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return typeWork.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create a type of work, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var typeWork = _database.TypeWorks.GetById(id);

                if (typeWork is not null)
                {
                    try
                    {
                        _database.TypeWorks.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete a type of work, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete a type of work, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<TypeWorkDTO> Find(Func<TypeWork, bool> predicate)
        {
            return _mapper.Map<IEnumerable<TypeWorkDTO>>(_database.TypeWorks.Find(predicate));
        }

        public IEnumerable<TypeWorkDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<TypeWorkDTO>>(_database.TypeWorks.GetAll());
        }

        public TypeWorkDTO GetById(int id, int? secondId = null)
        {
            var typeWork = _database.TypeWorks.GetById(id);

            if (typeWork is not null)
            {
                return _mapper.Map<TypeWorkDTO>(typeWork);
            }
            else
            {
                return null;
            }
        }

        public void Update(TypeWorkDTO item)
        {
            if (item is not null)
            {
                _database.TypeWorks.Update(_mapper.Map<TypeWork>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update a type of work, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update a type of work, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}