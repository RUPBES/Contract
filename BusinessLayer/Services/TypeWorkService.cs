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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.TypeWorks.GetById(item.Id) is null)
                {
                    var typeWork = _mapper.Map<TypeWork>(item);

                    _database.TypeWorks.Create(typeWork);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create a type of work, ID={typeWork.Id}",
                            nameSpace: typeof(TypeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return typeWork.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create a type of work, object is null",
                            nameSpace: typeof(TypeWorkService).Name,
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
                var typeWork = _database.TypeWorks.GetById(id);

                if (typeWork is not null)
                {
                    try
                    {
                        _database.TypeWorks.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete a type of work, ID={id}",
                            nameSpace: typeof(TypeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(TypeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete a type of work, ID is not more than zero",
                            nameSpace: typeof(TypeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.TypeWorks.Update(_mapper.Map<TypeWork>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update a type of work, ID={item.Id}",
                            nameSpace: typeof(TypeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update a type of work, object is null",
                            nameSpace: typeof(TypeWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }
    }
}