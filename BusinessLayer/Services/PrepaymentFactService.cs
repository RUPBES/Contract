using AutoMapper;
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
                    _logger.WriteLog(LogLevel.Information, $"create prepayment fact, ID={prepPlan.Id}", typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

                    return prepPlan.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create prepayment fact, object is null", typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);

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
                        _logger.WriteLog(LogLevel.Information, $"delete prepayment plan, ID={id}", typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete prepayment plan, ID is not more than zero", typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update prepayment fact, ID={item.Id}", typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update prepayment fact, object is null", typeof(PrepaymentFactService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<PrepaymentFactDTO> Find(Func<PrepaymentFact, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PrepaymentFactDTO>>(_database.PrepaymentFacts.Find(predicate));
        }
    }
}