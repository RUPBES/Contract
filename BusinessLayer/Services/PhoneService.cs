using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace BusinessLayer.Services
{
    internal class PhoneService : IPhoneService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public PhoneService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public void Create(PhoneDTO item)
        {
            if (item is not null)
            {
                if (_database.Phones.GetById(item.Id) is null)
                {
                    var phone = _mapper.Map<Phone>(item);

                    _database.Phones.Create(phone);
                    _database.Save();
                }
            }
        }

        public void Delete(int id)
        {
            _database.Phones.Delete(id);
            _database.Save();
        }

        public IEnumerable<PhoneDTO> Find(Func<Phone, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PhoneDTO>>(_database.Phones.Find(predicate));
        }

        public IEnumerable<PhoneDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PhoneDTO>>(_database.Phones.GetAll());
        }

        public PhoneDTO GetById(int id)
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
            }
        }
    }
}
