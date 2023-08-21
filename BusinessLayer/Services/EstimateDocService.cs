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
            if (item is not null)
            {
                if (_database.EstimateDocs.GetById(item.Id) is null)
                {
                    var estimate = _mapper.Map<EstimateDoc>(item);

                    _database.EstimateDocs.Create(estimate);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create estimate documentation, ID={estimate.Id}", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return estimate.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create estimate documentation, object is null", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var estimate = _database.EstimateDocs.GetById(id);

                if (estimate is not null)
                {
                    try
                    {
                        _database.EstimateDocs.Delete(id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"delete estimate documentation, ID={id}", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete estimate documentation, ID is not more than zero", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
            if (item is not null)
            {
                _database.EstimateDocs.Update(_mapper.Map<EstimateDoc>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update estimate documentation, ID={item.Id}", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update estimate documentation, object is null", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<EstimateDocDTO> Find(Func<EstimateDoc, bool> predicate)
        {
            return _mapper.Map<IEnumerable<EstimateDocDTO>>(_database.EstimateDocs.Find(predicate));
        }

        public void AddFile(int estimateDocId, int fileId)
        {
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
                    _logger.WriteLog(LogLevel.Information, $"create file of an estimate documentation", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create file of an estimate documentation, object is null", typeof(EstimateDocService).Name, MethodBase.GetCurrentMethod()?.Name, _http?.HttpContext?.User?.Identity?.Name);
        }
    }
}