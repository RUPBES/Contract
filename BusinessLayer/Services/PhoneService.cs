using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class PhoneService : IPhoneService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public PhoneService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(PhoneDTO item)
        {
            if (item is not null)
            {
                if (_database.Phones.GetById(item.Id) is null)
                {
                    var phone = _mapper.Map<Phone>(item);

                    _database.Phones.Create(phone);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create phone, ID={phone.Id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return phone.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create phone, object is null",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var phone = _database.Phones.GetById(id);

                if (phone is not null)
                {
                    try
                    {
                        _database.Phones.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete phone, ID={id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete phone, ID is not more than zero",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<PhoneDTO> Find(Func<Phone, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PhoneDTO>>(_database.Phones.Find(predicate));
        }

        public IEnumerable<PhoneDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PhoneDTO>>(_database.Phones.GetAll());
        }

        public PhoneDTO GetById(int id, int? secondId = null)
        {
            var phone = _database.Phones.GetById(id);

            if (phone is not null)
            {
                return _mapper.Map<PhoneDTO>(phone);
            }
            else
            {
                return null;
            }
        }

        public void Update(PhoneDTO item)
        {
            if (item is not null)
            {
                _database.Phones.Update(_mapper.Map<Phone>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update phone, ID={item.Id}",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update phone, object is null",
                            nameSpace: typeof(OrganizationService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
