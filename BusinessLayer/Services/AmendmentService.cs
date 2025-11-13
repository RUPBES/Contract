using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    public class AmendmentService : IAmendmentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly ILoggerContract _logger;

        public AmendmentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _databaseArch = databaseArch;
        }

        public int? Create(AmendmentDTO item)
        {
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
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return amend.Id;
                }
            }

            _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not create amendment, object is null",
                           nameSpace: typeof(AmendmentService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
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
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete amendment, ID is not more than zero",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
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
            if (item is not null)
            {
                _database.Amendments.Update(_mapper.Map<Amendment>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update amendment, ID={item.Id}",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update amendment, object is null",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<AmendmentDTO> Find(Func<Amendment, bool> predicate, bool? useArchiveData)
        {
            return (useArchiveData == true) ?
                _mapper.Map<IEnumerable<AmendmentDTO>>(_databaseArch.Amendments.Find(predicate)):
                _mapper.Map<IEnumerable<AmendmentDTO>>(_database.Amendments.Find(predicate));
        }

        public IEnumerable<AmendmentDTO> Find(Func<Amendment, bool> where, Func<Amendment, Amendment> select, bool useArchiveData)
        {
            var amendments = useArchiveData == true ?
                               _databaseArch.Amendments.Find(where, select)
                               : _database.Amendments.Find(where, select);
            return _mapper.Map<IEnumerable<AmendmentDTO>>(amendments);
        }

        public void AddFile(int amendId, int fileId)
        {
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
                            methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file of amendment, object is null",
                            nameSpace: typeof(AmendmentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}