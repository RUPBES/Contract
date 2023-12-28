using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace BusinessLayer.Services
{
    internal class SelectionProcedureService : ISelectionProcedureService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public SelectionProcedureService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(SelectionProcedureDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.SelectionProcedures.GetById(item.Id) is null)
                {
                    var selectionProcedure = _mapper.Map<SelectionProcedure>(item);

                    _database.SelectionProcedures.Create(selectionProcedure);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create selection procedure, ID={selectionProcedure.Id}",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return selectionProcedure.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create selection procedure, object is null",
                            nameSpace: typeof(SelectionProcedureService).Name,
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
                var selectionProcedure = _database.SelectionProcedures.GetById(id);

                if (selectionProcedure is not null)
                {
                    try
                    {
                        _database.SelectionProcedures.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete selection procedure, ID={id}",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete selection procedure, ID is not more than zero",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<SelectionProcedureDTO> Find(Func<SelectionProcedure, bool> predicate)
        {
            return _mapper.Map<IEnumerable<SelectionProcedureDTO>>(_database.SelectionProcedures.Find(predicate));
        }

        public IEnumerable<SelectionProcedureDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<SelectionProcedureDTO>>(_database.SelectionProcedures.GetAll());
        }

        public SelectionProcedureDTO GetById(int id, int? secondId = null)
        {
            var selectionProcedure = _database.SelectionProcedures.GetById(id);

            if (selectionProcedure is not null)
            {
                return _mapper.Map<SelectionProcedureDTO>(selectionProcedure);
            }
            else
            {
                return null;
            }
        }

        public void Update(SelectionProcedureDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.SelectionProcedures.Update(_mapper.Map<SelectionProcedure>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update selection procedure, ID={item.Id}",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update selection procedure, object is null",
                            nameSpace: typeof(SelectionProcedureService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }
    }
}