using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class PrepaymentFactService: IPrepaymentFactService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public PrepaymentFactService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(PrepaymentFactDTO item)
        {
            if (item is not null)
            {
                if (_database.PrepaymentFacts.GetById(item.Id) is null)
                {
                    var prepPlan = _mapper.Map<PrepaymentFact>(item);

                    _database.PrepaymentFacts.Create(prepPlan);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create prepayment fact, ID={prepPlan.Id}",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return prepPlan.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create prepayment fact, object is null",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var model = _database.PrepaymentFacts.GetById(id);

                if (model is not null)
                {
                    try
                    {
                        _database.PrepaymentFacts.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete prepayment plan, ID={id}",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete prepayment plan, ID is not more than zero",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<PrepaymentFactDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PrepaymentFactDTO>>(_database.PrepaymentFacts.GetAll());
        }

        public PrepaymentFactDTO GetById(int id, int? secondId = null)
        {
            var act = _database.PrepaymentFacts.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<PrepaymentFactDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(PrepaymentFactDTO item)
        {
            if (item is not null)
            {
                _database.PrepaymentFacts.Update(_mapper.Map<PrepaymentFact>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update prepayment fact, ID={item.Id}",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update prepayment fact, object is null",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);                
            }
        }

        public IEnumerable<PrepaymentFactDTO> Find(Func<PrepaymentFact, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentFactDTO>>(_database.PrepaymentFacts.Find(predicate));
        }

        public Prepayment GetLastPrepayment(int contractId)
        {
            try
            {
                var prep = _database.Prepayments.Find(a => a.ContractId == contractId && a.IsChange != true).FirstOrDefault();
                if (prep == null)
                    return null;
                return prep;
            }
            catch { return null; }
        }
    }
}