using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return prepPlan.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create prepayment fact, object is null",
                            nameSpace: typeof(PrepaymentFactService).Name,
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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(PrepaymentFactService).Name,
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
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.PrepaymentFacts.Update(_mapper.Map<PrepaymentFact>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update prepayment fact, ID={item.Id}",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update prepayment fact, object is null",
                            nameSpace: typeof(PrepaymentFactService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);                
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
                var list = _database.Prepayments.Find(a => a.ContractId == contractId && a.IsChange != true).ToList();
                List<(Prepayment, DateTime)> listSort = new List<(Prepayment, DateTime)>();
                foreach (var item in list)
                {
                    (Prepayment, DateTime) obj;
                    var ob = _database.PrepaymentAmendments.Find(s => s.PrepaymentId == item.Id).FirstOrDefault();
                    if (ob == null)
                        obj.Item2 = new DateTime(1900, 1, 1);
                    else obj.Item2 = (DateTime)_database.Amendments.Find(x => x.Id == ob.AmendmentId).Select(x => x.Date).FirstOrDefault();
                    obj.Item1 = item;
                    listSort.Add(obj);
                }
                listSort = listSort.OrderBy(x => x.Item2).ToList();

                return _mapper.Map<Prepayment>(listSort.Select(x => x.Item1).LastOrDefault());
            }
            catch { return null; }
        }
    }
}