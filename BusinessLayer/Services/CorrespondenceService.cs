using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class CorrespondenceService: ICorrespondenceService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;

        public CorrespondenceService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
        }

        public int? Create(CorrespondenceDTO item)
        {
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
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return corr.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create correspondence, object is null",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
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
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete correspondence, ID is not more than zero",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
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
            if (item is not null)
            {
                _database.Correspondences.Update(_mapper.Map<Correspondence>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update correspondence, ID={item.Id}",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update correspondence, object is null",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<CorrespondenceDTO> Find(Func<Correspondence, bool> predicate)
        {
            return _mapper.Map<IEnumerable<CorrespondenceDTO>>(_database.Correspondences.Find(predicate));
        }

        public void AddFile(int correspondenceId, int fileId)
        {
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
                            methodName: MethodBase.GetCurrentMethod().Name);
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of correspondence, object is null",
                            nameSpace: typeof(CorrespondenceService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
        }
    }
}