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
    public class EstimateDocService: IEstimateDocService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public EstimateDocService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(EstimateDocDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                if (_database.EstimateDocs.GetById(item.Id) is null)
                {
                    var estimate = _mapper.Map<EstimateDoc>(item);

                    _database.EstimateDocs.Create(estimate);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create estimate documentation, ID={estimate.Id}",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return estimate.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create estimate documentation, object is null",
                            nameSpace: typeof(EstimateDocService).Name,
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
                var estimate = _database.EstimateDocs.GetById(id);

                if (estimate is not null)
                {
                    try
                    {
                        _database.EstimateDocs.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete estimate documentation, ID={id}",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete estimate documentation, ID is not more than zero",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<EstimateDocDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<EstimateDocDTO>>(_database.EstimateDocs.GetAll());
        }

        public EstimateDocDTO GetById(int id, int? secondId = null)
        {
            var estimate = _database.EstimateDocs.GetById(id);

            if (estimate is not null)
            {
                return _mapper.Map<EstimateDocDTO>(estimate);
            }
            else
            {
                return null;
            }
        }

        public void Update(EstimateDocDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.EstimateDocs.Update(_mapper.Map<EstimateDoc>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update estimate documentation, ID={item.Id}",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update estimate documentation, object is null",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IEnumerable<EstimateDocDTO> Find(Func<EstimateDoc, bool> predicate)
        {
            return _mapper.Map<IEnumerable<EstimateDocDTO>>(_database.EstimateDocs.Find(predicate));
        }

        public void AddFile(int estimateDocId, int fileId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (fileId > 0 && estimateDocId > 0)
            {
                if (_database.EstimateDocFiles.GetById(estimateDocId, fileId) is null)
                {
                    _database.EstimateDocFiles.Create(new EstimateDocFile
                    {
                        EstimateDocId = estimateDocId,
                        FileId = fileId
                    });

                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file of an estimate documentation",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of an estimate documentation, object is null",
                            nameSpace: typeof(EstimateDocService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
        }
    }
}