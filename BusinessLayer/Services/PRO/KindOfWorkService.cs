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
    internal class KindOfWorkService : IKindOfWorkService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public KindOfWorkService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(KindOfWorkDTO item)
        {
            if (item is not null)
            {
                if (_database.KindOfWorks.GetById(item.Id) is null)
                {
                    var kindOfWork = _mapper.Map<KindOfWork>(item);
                    _database.KindOfWorks.Create(kindOfWork);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create KindOfWork, ID={kindOfWork.Id}",
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return kindOfWork.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create KindOfWork, object is null",
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var kindOfWork = _database.KindOfWorks.GetById(id);

                if (kindOfWork is not null)
                {
                    try
                    {
                        _database.KindOfWorks.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete KindOfWork, ID={id}",
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete KindOfWork, ID is not more than zero",
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<KindOfWorkDTO> Find(Func<KindOfWork, bool> predicate)
        {
            return _mapper.Map<IEnumerable<KindOfWorkDTO>>(_database.KindOfWorks.Find(predicate));
        }

        public IEnumerable<KindOfWorkDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<KindOfWorkDTO>>(_database.KindOfWorks.GetAll());
        }

        public KindOfWorkDTO GetById(int id, int? secondId = null)
        {
            var kindOfWork = _database.KindOfWorks.GetById(id);

            if (kindOfWork is not null)
            {
                return _mapper.Map<KindOfWorkDTO>(kindOfWork);
            }
            else
            {
                return null;
            }
        }

        public void Update(KindOfWorkDTO item)
        {
            if (item is not null)
            {
                _database.KindOfWorks.Update(_mapper.Map<KindOfWork>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update KindOfWork, ID={item.Id}",
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update KindOfWork, object is null",
                            nameSpace: typeof(KindOfWorkService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
