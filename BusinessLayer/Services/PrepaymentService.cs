﻿using AutoMapper;
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
    internal class PrepaymentService: IPrepaymentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public PrepaymentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(PrepaymentDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.Prepayments.GetById(item.Id) is null)
                {
                    var service = _mapper.Map<Prepayment>(item);

                    _database.Prepayments.Create(service);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create prepayment, ID={service.Id}",
                            nameSpace: typeof(PrepaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return service.Id;
                }
            }

            _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not create prepayment, object is null",
                           nameSpace: typeof(PrepaymentService).Name,
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
                var service = _database.Prepayments.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.Prepayments.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"delete prepayment, ID={id}",
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                           logLevel: LogLevel.Error,
                           message: e.Message,
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not delete prepayment, ID is not more than zero",
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
            }
        }

        public IEnumerable<PrepaymentDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PrepaymentDTO>>(_database.Prepayments.GetAll());
        }

        public PrepaymentDTO GetById(int id, int? secondId = null)
        {
            var act = _database.Prepayments.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<PrepaymentDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Prepayments.Update(_mapper.Map<Prepayment>(item));
                _database.Save();

                _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"update prepayment, ID={item.Id}",
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
            }
            else
            {
                _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not update prepayment, object is null",
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
            }
        }

        public IEnumerable<PrepaymentDTO> Find(Func<Prepayment, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentDTO>>(_database.Prepayments.Find(predicate));
        }

        public IEnumerable<PrepaymentDTO> FindByContractId(int id)
        {
            return _mapper.Map<IEnumerable<PrepaymentDTO>>(_database.Prepayments.GetAll().Where(p => p.ContractId == id));
        }

        public AmendmentDTO? GetAmendmentByPrepaymentId(int prepaymentId)
        {
            var prepModelChange = _database.Prepayments.GetById(prepaymentId);
            return _mapper.Map<AmendmentDTO>(_database.PrepaymentAmendments?.Find(p => p.PrepaymentId == prepModelChange.ChangePrepaymentId)?.FirstOrDefault()?.Amendment);
        }

        public void AddAmendmentToPrepayment(int amendmentId, int prepaymentId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (amendmentId > 0 && prepaymentId > 0)
            {
                _database.PrepaymentAmendments.Create(new PrepaymentAmendment 
                { 
                    AmendmentId = amendmentId, 
                    PrepaymentId = prepaymentId
                });

                _database.Save();

                _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"add amendment (ID={amendmentId}) to prepayment (ID={prepaymentId})",
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
            }
            else
            {
                _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not add prepaymentAmendments",
                           nameSpace: typeof(PrepaymentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name,
                           userName: user);
            }
        }

        public IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId)
        {
            var list = _database.Amendments.Find(a => a.ContractId == contractId).ToList();
            List<Amendment> answer = new List<Amendment>();
            foreach (var item in list)
            {
                var ob = _database.PrepaymentAmendments.Find(s => s.AmendmentId == item.Id).FirstOrDefault();
                if (ob == null)
                    answer.Add(item);
            }
            return _mapper.Map<IEnumerable<AmendmentDTO>>(list);
        }

        public Prepayment GetLastPrepayment(int contractId)
        {
            var list = _database.Prepayments.Find(a => a.ContractId == contractId).ToList();
            List<(Prepayment, DateTime)> listSort = new List<(Prepayment, DateTime)>();
            foreach (var item in list)
            {
                (Prepayment, DateTime) obj;
                var ob = _database.PrepaymentAmendments.Find(s => s.PrepaymentId == item.Id).FirstOrDefault();
                if (ob == null)
                    obj.Item2 = new DateTime(1900, 1, 1);
                else obj.Item2 = (DateTime )_database.Amendments.Find(x => x.Id == ob.AmendmentId).Select(x => x.Date).FirstOrDefault();
                obj.Item1 = item;
                listSort.Add(obj);
            }
            listSort = listSort.OrderBy(x => x.Item2).ToList(); 
            return _mapper.Map<Prepayment>(listSort.Select(x => x.Item1).LastOrDefault());
        }

        public Prepayment GetPrepaymentByAmendment(int amendmentId)
        {
            if (amendmentId != 0)
            {
                var prepaymentId = _database.PrepaymentAmendments.Find(a => a.AmendmentId == amendmentId).Select(a => a.PrepaymentId).FirstOrDefault();
                if (prepaymentId != null && prepaymentId != 0)
                {
                    return _database.Prepayments.GetById(prepaymentId);
                }
                return null;
            }
            else return null;
        }
    }
}