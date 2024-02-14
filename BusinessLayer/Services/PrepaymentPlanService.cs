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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.PrepaymentPlans.GetById(item.Id) is null)
                {
                    var prepPlan = _mapper.Map<PrepaymentPlan>(item);

                    _database.PrepaymentPlans.Create(prepPlan);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create prepayment, ID={prepPlan.Id}",
                            nameSpace: typeof(PrepaymentPlanService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return prepPlan.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create prepayment, object is null",
                            nameSpace: typeof(PrepaymentPlanService).Name,
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
                var service = _database.PrepaymentPlans.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.PrepaymentPlans.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete prepayment plan, ID={id}",
                            nameSpace: typeof(PrepaymentPlanService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(PrepaymentPlanService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete prepayment plan, ID is not more than zero",
                            nameSpace: typeof(PrepaymentPlanService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.PrepaymentPlans.Update(_mapper.Map<PrepaymentPlan>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update prepayment plan, ID={item.Id}",
                            nameSpace: typeof(PrepaymentPlanService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update prepayment plan, object is null",
                            nameSpace: typeof(PrepaymentPlanService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<PrepaymentPlanDTO> Find(Func<PrepaymentPlan, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentPlanDTO>>(_database.PrepaymentPlans.Find(predicate));
        }
    }
}
