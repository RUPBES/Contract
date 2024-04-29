using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using DatabaseLayer.Models.KDO;
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
    internal class CorrespondenceService: ICorrespondenceService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public CorrespondenceService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(CorrespondenceDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.Correspondences.GetById(item.Id) is null)
                {
                    var corr = _mapper.Map<Correspondence>(item);

                    _database.Correspondences.Create(corr);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create correspondence, ID={corr.Id}",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return corr.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create correspondence, object is null",
                            nameSpace: typeof(CorrespondenceService).Name,
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
                var corr = _database.Correspondences.GetById(id);

                if (corr is not null)
                {
                    try
                    {
                        _database.Correspondences.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete correspondence, ID={id}",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete correspondence, ID is not more than zero",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<CorrespondenceDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<CorrespondenceDTO>>(_database.Correspondences.GetAll());
        }

        public CorrespondenceDTO GetById(int id, int? secondId = null)
        {
            var corr = _database.Correspondences.GetById(id);

            if (corr is not null)
            {
                return _mapper.Map<CorrespondenceDTO>(corr);
            }
            else
            {
                return null;
            }
        }

        public void Update(CorrespondenceDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Correspondences.Update(_mapper.Map<Correspondence>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update correspondence, ID={item.Id}",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update correspondence, object is null",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<CorrespondenceDTO> Find(Func<Correspondence, bool> predicate)
        {
            return _mapper.Map<IEnumerable<CorrespondenceDTO>>(_database.Correspondences.Find(predicate));
        }

        public void AddFile(int correspondenceId, int fileId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (fileId > 0 && correspondenceId > 0)
            {
                if (_database.CorrespondenceFiles.GetById(correspondenceId, fileId) is null)
                {
                    _database.CorrespondenceFiles.Create(new CorrespondenceFile
                    {
                        CorrespondenceId = correspondenceId,
                        FileId = fileId
                    });

                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of correspondence",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of correspondence, object is null",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
        }
    }
}