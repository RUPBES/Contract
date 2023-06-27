using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    internal class AddressService : IAddressService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public AddressService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
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
                    return address.Id;
                }
            }
            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            _database.Addresses.Delete(id);
            _database.Save();
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
            }
        }
    }
}
