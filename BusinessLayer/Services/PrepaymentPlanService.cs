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
    internal class PrepaymentPlanService : IPrepaymentPlanService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public PrepaymentPlanService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(PrepaymentPlanDTO item)
        {
            if (item is not null)
            {
                if (_database.PrepaymentPlans.GetById(item.Id) is null)
                {
                    var prepPlan = _mapper.Map<PrepaymentPlan>(item);

                    _database.PrepaymentPlans.Create(prepPlan);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create prepayment, ID={prepPlan.Id}", typeof(PrepaymentPlanService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

                    return prepPlan.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create prepayment, object is null", typeof(PrepaymentPlanService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var service = _database.PrepaymentPlans.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.PrepaymentPlans.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete prepayment plan, ID={id}", typeof(PrepaymentPlanService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(PrepaymentPlanService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete prepayment plan, ID is not more than zero", typeof(PrepaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<PrepaymentPlanDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PrepaymentPlanDTO>>(_database.PrepaymentPlans.GetAll());
        }

        public PrepaymentPlanDTO GetById(int id, int? secondId = null)
        {
            var act = _database.PrepaymentPlans.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<PrepaymentPlanDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentPlanDTO item)
        {
            if (item is not null)
            {
                _database.PrepaymentPlans.Update(_mapper.Map<PrepaymentPlan>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update prepayment plan, ID={item.Id}", typeof(PrepaymentPlanService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update prepayment plan, object is null", typeof(PrepaymentPlanService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<PrepaymentPlanDTO> Find(Func<PrepaymentPlan, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentPlanDTO>>(_database.PrepaymentPlans.Find(predicate));
        }
    }
}
