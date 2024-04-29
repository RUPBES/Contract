using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class CommissionActService: ICommissionActService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public CommissionActService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(CommissionActDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.CommissionActs.GetById(item.Id) is null)
                {
                    var comAct = _mapper.Map<CommissionAct>(item);

                    _database.CommissionActs.Create(comAct);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create commission act, ID={comAct.Id}",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return comAct.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create commission act, object is null",
                            nameSpace: typeof(CommissionActService).Name,
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
                var comAct = _database.CommissionActs.GetById(id);

                if (comAct is not null)
                {
                    try
                    {
                        _database.CommissionActs.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete commission act, ID={id}",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete commission act, ID is not more than zero",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<CommissionActDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<CommissionActDTO>>(_database.CommissionActs.GetAll());
        }

        public CommissionActDTO GetById(int id, int? secondId = null)
        {
            var comAct = _database.CommissionActs.GetById(id);

            if (comAct is not null)
            {
                return _mapper.Map<CommissionActDTO>(comAct);
            }
            else
            {
                return null;
            }
        }

        public void Update(CommissionActDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.CommissionActs.Update(_mapper.Map<CommissionAct>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update commission act, ID={item.Id}",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update commission act, object is null",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<CommissionActDTO> Find(Func<CommissionAct, bool> predicate)
        {
            return _mapper.Map<IEnumerable<CommissionActDTO>>(_database.CommissionActs.Find(predicate));
        }

        public void AddFile(int commissionActId, int fileId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (fileId > 0 && commissionActId > 0)
            {
                if (_database.CommissionActFiles.GetById(commissionActId, fileId) is null)
                {
                    _database.CommissionActFiles.Create(new CommissionActFile
                    {
                        СommissionActId = commissionActId,
                        FileId = fileId
                    });

                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of a commission act",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of a commission act, object is null",
                            nameSpace: typeof(CommissionActService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
        }
    }
}