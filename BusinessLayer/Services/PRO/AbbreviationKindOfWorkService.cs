using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services.PRO
{
    internal class AbbreviationKindOfWorkService : IAbbreviationKindOfWorkService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public AbbreviationKindOfWorkService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(AbbreviationKindOfWorkDTO item)
        {
            if (item is not null)
            {
                if (_database.AbbreviationKindOfWorks.GetById(item.Id) is null)
                {
                    var abbreviationKindOfWork = _mapper.Map<AbbreviationKindOfWork>(item);
                    _database.AbbreviationKindOfWorks.Create(abbreviationKindOfWork);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create AbbreviationKindOfWork, ID={abbreviationKindOfWork.Id}",
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return abbreviationKindOfWork.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create AbbreviationKindOfWork, object is null",
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var abbreviationKindOfWork = _database.AbbreviationKindOfWorks.GetById(id);

                if (abbreviationKindOfWork is not null)
                {
                    try
                    {
                        _database.AbbreviationKindOfWorks.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete AbbreviationKindOfWork, ID={id}",
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete AbbreviationKindOfWork, ID is not more than zero",
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<AbbreviationKindOfWorkDTO> Find(Func<AbbreviationKindOfWork, bool> predicate)
        {
            return _mapper.Map<IEnumerable<AbbreviationKindOfWorkDTO>>(_database.AbbreviationKindOfWorks.Find(predicate));
        }

        public IEnumerable<AbbreviationKindOfWorkDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<AbbreviationKindOfWorkDTO>>(_database.AbbreviationKindOfWorks.GetAll());
        }

        public AbbreviationKindOfWorkDTO GetById(int id, int? secondId = null)
        {
            var abbreviationKindOfWork = _database.AbbreviationKindOfWorks.GetById(id);

            if (abbreviationKindOfWork is not null)
            {
                return _mapper.Map<AbbreviationKindOfWorkDTO>(abbreviationKindOfWork);
            }
            else
            {
                return null;
            }
        }

        public void Update(AbbreviationKindOfWorkDTO item)
        {
            if (item is not null)
            {
                _database.AbbreviationKindOfWorks.Update(_mapper.Map<AbbreviationKindOfWork>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update AbbreviationKindOfWork, ID={item.Id}",
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update AbbreviationKindOfWork, object is null",
                            nameSpace: typeof(AbbreviationKindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
