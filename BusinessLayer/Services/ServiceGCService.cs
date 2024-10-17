using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class ServiceGCService : IServiceGCService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public ServiceGCService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(ServiceGCDTO item)
        {
            if (item is not null)
            {
                if (_database.ServiceGCs.GetById(item.Id) is null)
                {
                    var service = _mapper.Map<ServiceGc>(item);

                    _database.ServiceGCs.Create(service);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create service of general contractor, ID={service.Id}",
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return service.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create service of general contractor, object is null",
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var service = _database.ServiceGCs.GetById(id);

                if (service is not null)
                {
                    try
                    {
                        _database.ServiceGCs.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete service of general contractor, ID={id}",
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete service of general contractor, ID is not more than zero",
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<ServiceGCDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ServiceGCDTO>>(_database.ServiceGCs.GetAll());
        }

        public ServiceGCDTO GetById(int id, int? secondId = null)
        {
            var act = _database.ServiceGCs.GetById(id);

            if (act is not null)
            {
                return _mapper.Map<ServiceGCDTO>(act);
            }
            else
            {
                return null;
            }
        }

        public void Update(ServiceGCDTO item)
        {
            if (item is not null)
            {
                _database.ServiceGCs.Update(_mapper.Map<ServiceGc>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update service of general contractor, ID={item.Id}",
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update service of general contractor, object is null",
                            nameSpace: typeof(ServiceGCService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<ServiceGCDTO> Find(Func<ServiceGc, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ServiceGCDTO>>(_database.ServiceGCs.Find(predicate));
        }

        public void AddAmendmentToService(int amendmentId, int serviceId)
        {
            if (amendmentId > 0 && serviceId > 0)
            {
                _database.ServiceAmendments.Create(new ServiceAmendment
                {
                    AmendmentId = amendmentId,
                    ServiceId = serviceId
                });

                _database.Save();

                _logger.WriteLog(
                           logLevel: LogLevel.Information,
                           message: $"add amendment (ID={amendmentId}) to service gencontractor (ID={serviceId})",
                           nameSpace: typeof(ServiceGCService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                           logLevel: LogLevel.Warning,
                           message: $"not add serviceAmendment",
                           nameSpace: typeof(ServiceGCService).Name,
                           methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId)
        {
            var list = _database.Amendments.Find(a => a.ContractId == contractId).ToList();
            List<Amendment> answer = new List<Amendment>();
            foreach (var item in list)
            {
                var ob = _database.ServiceAmendments.Find(s => s.AmendmentId == item.Id).FirstOrDefault();
                if (ob == null)
                    answer.Add(item);
            }
            return _mapper.Map<IEnumerable<AmendmentDTO>>(answer);
        }
    }
}
