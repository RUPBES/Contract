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
    internal class AddressService : IAddressService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;

        public AddressService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
        }

        public int? Create(AddressDTO item)
        {
            if (item is not null)
            {
                if (_database.Addresses.GetById(item.Id) is null)
                {
                    var address = _mapper.Map<Address>(item);

                    _database.Addresses.Create(address);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create address, ID={address.Id}, Name={address.FullAddress}",
                            nameSpace: typeof(AddressService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return address.Id;
                }
            }

            _logger.WriteLog(                            
                logLevel: LogLevel.Warning,
                message: $"not create address, object is null",
                nameSpace: typeof(AddressService).Name,
                methodName: MethodBase.GetCurrentMethod().Name);            

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var address = _database.Addresses.GetById(id);

                if (address is not null)
                {
                    try
                    {
                        _database.Addresses.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete address, ID={id}",
                            nameSpace: typeof(AddressService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(AddressService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete address, ID is not more than zero",
                            nameSpace: typeof(AddressService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<AddressDTO> Find(Func<Address, bool> predicate)
        {
            return _mapper.Map<IEnumerable<AddressDTO>>(_database.Addresses.Find(predicate));
        }

        public IEnumerable<AddressDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<AddressDTO>>(_database.Addresses.GetAll());
        }

        public AddressDTO GetById(int id, int? secondId = null)
        {
            var address = _database.Addresses.GetById(id);

            if (address is not null)
            {
                return _mapper.Map<AddressDTO>(address);
            }
            else
            {
                return null;
            }
        }

        public void Update(AddressDTO item)
        {
            if (item is not null)
            {
                _database.Addresses.Update(_mapper.Map<Address>(item));
                _database.Save();

                _logger.WriteLog(
                    logLevel: LogLevel.Information,
                    message: $"update address, ID={item.Id}",
                    nameSpace: typeof(AddressService).Name,
                    methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                    logLevel: LogLevel.Warning,
                    message: $"not update address, object is null",
                    nameSpace: typeof(AddressService).Name,
                    methodName: MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
