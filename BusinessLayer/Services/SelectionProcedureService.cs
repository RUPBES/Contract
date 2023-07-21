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
            if (item is not null)
            {
                if (_database.SelectionProcedures.GetById(item.Id) is null)
                {
                    var selectionProcedure = _mapper.Map<SelectionProcedure>(item);

                    _database.SelectionProcedures.Create(selectionProcedure);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create selection procedure, ID={selectionProcedure.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return selectionProcedure.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create selection procedure, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var selectionProcedure = _database.SelectionProcedures.GetById(id);

                if (selectionProcedure is not null)
                {
                    try
                    {
                        _database.SelectionProcedures.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete selection procedure, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete selection procedure, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            if (item is not null)
            {
                _database.SelectionProcedures.Update(_mapper.Map<SelectionProcedure>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update selection procedure, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update selection procedure, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}