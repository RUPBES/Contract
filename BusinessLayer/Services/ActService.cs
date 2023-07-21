using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    internal class ActService: IActService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public ActService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(ActDTO item)
        {
            if (item is not null)
            {
                if (_database.Acts.GetById(item.Id) is null)
                {
                    var act = _mapper.Map<Act>(item);

                    _database.Acts.Create(act);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create act, ID={act.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return act.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create act, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var act = _database.Acts.GetById(id);

                if (act is not null)
                {
                    try
                    {
                        _database.Acts.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete act, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete act, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }


        public IEnumerable<ActDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ActDTO>>(_database.Acts.GetAll());
        }

        public ActDTO GetById(int id, int? secondId = null)
        {
            var act = _database.Acts.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<ActDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(ActDTO item)
        {
            if (item is not null)
            {
                _database.Acts.Update(_mapper.Map<Act>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update act, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update act, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<ActDTO> Find(Func<Act, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ActDTO>>(_database.Acts.Find(predicate));
        }
    }
}