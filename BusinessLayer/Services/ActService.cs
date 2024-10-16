﻿using AutoMapper;
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            if (item is not null)
            {
                if (_database.Acts.GetById(item.Id) is null)
                {
                    var act = _mapper.Map<Act>(item);

                    _database.Acts.Create(act);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create act, ID={act.Id}",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    return act.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create act, object is null",
                            nameSpace: typeof(ActService).Name,
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
                var act = _database.Acts.GetById(id);

                if (act is not null)
                {
                    try
                    {
                        _database.Acts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete act, ID={id}",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete act, ID is not more than zero",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Acts.Update(_mapper.Map<Act>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update act, ID={item.Id}",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);                
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update act, object is null",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<ActDTO> Find(Func<Act, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ActDTO>>(_database.Acts.Find(predicate));
        }

        public void AddFile(int actId, int fileId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (fileId > 0 && actId > 0)
            {
                if (_database.ActFiles.GetById(actId, fileId) is null)
                {
                    _database.ActFiles.Create(new ActFile
                    {
                        ActId = actId,
                        FileId = fileId
                    });

                    _database.Save();
                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of act",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of act, object is null",
                            nameSpace: typeof(ActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
        }
    }
}