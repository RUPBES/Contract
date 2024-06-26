using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;

namespace BusinessLayer.Services
{
    public class AmendmentService : IAmendmentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public AmendmentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(AmendmentDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.Amendments.GetById(item.Id) is null)
                {
                    var amend = _mapper.Map<Amendment>(item);

                    _database.Amendments.Create(amend);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create amendment, ID={amend.Id}",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return amend.Id;
                }
            }

            _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not create amendment, object is null",
                           nameSpace: typeof(AmendmentService).Name,
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
                var act = _database.Amendments.GetById(id);

                if (act is not null)
                {
                    try
                    {
                        _database.Amendments.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete amendment, ID={id}",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete amendment, ID is not more than zero",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<AmendmentDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<AmendmentDTO>>(_database.Amendments.GetAll());
        }

        public AmendmentDTO GetById(int id, int? secondId = null)
        {
            var act = _database.Amendments.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<AmendmentDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(AmendmentDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Amendments.Update(_mapper.Map<Amendment>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update amendment, ID={item.Id}",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update amendment, object is null",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<AmendmentDTO> Find(Func<Amendment, bool> predicate)
        {
            return _mapper.Map<IEnumerable<AmendmentDTO>>(_database.Amendments.Find(predicate));
        }

        public IEnumerable<AmendmentDTO> Find(Func<Amendment, bool> where, Func<Amendment, Amendment> select)
        {
            return _mapper.Map<IEnumerable<AmendmentDTO>>(_database.Amendments.Find(where, select));
        }

        public void AddFile(int amendId, int fileId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (fileId > 0 && amendId > 0)
            {
                if (_database.AmendmentFiles.GetById(amendId, fileId) is null)
                {
                    _database.AmendmentFiles.Create(new AmendmentFile
                    {
                        AmendmentId = amendId,
                        FileId = fileId
                    });

                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of amendment",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of amendment, object is null",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public (DateTime?, DateTime?)? GetPeriodRangeOfContractById(int contractId)
        {
            (DateTime?, DateTime?) range = (null, null);

            var lastAmendment = _database.Amendments.Find(x => x.ContractId == contractId).LastOrDefault();
            range.Item1 = lastAmendment.DateBeginWork;
            range.Item2 = lastAmendment.DateEndWork;

            if (range.Item1 is null && range.Item2 is null)
            {
                return null;
            }
            return range;
        }

        public bool? IsThereScopeWorkWitnLastAmendmentByContractId(int contractId)
        {
            var amendmentId = _database.Amendments.Find(x => x.ContractId == contractId && x.Type == "scope")?.LastOrDefault()?.Id;
            
            if (amendmentId is null)
            {
                return null;
            }
            
            return _database.ScopeWorkAmendments?.Find(p => p.AmendmentId == amendmentId)?.FirstOrDefault() is not null? true : false;
        }
    }
}