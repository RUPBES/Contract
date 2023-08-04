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
            if (item is not null)
            {
                if (_database.Prepayments.GetById(item.Id) is null)
                {
                    var service = _mapper.Map<Prepayment>(item);

                    _database.Prepayments.Create(service);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create prepayment, ID={service.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

                    return service.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create prepayment, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var service = _database.Prepayments.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.Prepayments.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete prepayment, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete prepayment, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            if (item is not null)
            {
                _database.Prepayments.Update(_mapper.Map<Prepayment>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update prepayment, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update prepayment, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<PrepaymentDTO> Find(Func<Prepayment, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentDTO>>(_database.Prepayments.Find(predicate));
        }

        public IEnumerable<PrepaymentDTO> FindByIdContract(int id)
        {
            return _mapper.Map<IEnumerable<PrepaymentDTO>>(GetAll().Where(p => p.ContractId == id));
        }
    }
}