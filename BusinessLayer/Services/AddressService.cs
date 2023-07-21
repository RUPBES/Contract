using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
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
    internal class AddressService : IAddressService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public AddressService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
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
                    _logger.WriteLog(LogLevel.Information, $"create address, ID={address.Id}, Name={address.FullAddress}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return address.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create address, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

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
                        _logger.WriteLog(LogLevel.Information, $"delete address, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete address, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update address, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update address, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}
