using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;

namespace BusinessLayer.Services
{
    internal class TypeWorkService : ITypeWorkService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public TypeWorkService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public int? Create(TypeWorkDTO item)
        {
            if (item is not null)
            {
                if (_database.TypeWorks.GetById(item.Id) is null)
                {
                    var typeWork = _mapper.Map<TypeWork>(item);

                    _database.TypeWorks.Create(typeWork);
                    _database.Save();
                    return typeWork.Id;
                }
            }
            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            _database.TypeWorks.Delete(id);
            _database.Save();
        }

        public IEnumerable<TypeWorkDTO> Find(Func<TypeWork, bool> predicate)
        {
            return _mapper.Map<IEnumerable<TypeWorkDTO>>(_database.TypeWorks.Find(predicate));
        }

        public IEnumerable<TypeWorkDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<TypeWorkDTO>>(_database.TypeWorks.GetAll());
        }

        public TypeWorkDTO GetById(int id, int? secondId = null)
        {
            var typeWork = _database.TypeWorks.GetById(id);

            if (typeWork is not null)
            {
                return _mapper.Map<TypeWorkDTO>(typeWork);
            }
            else
            {
                return null;
            }
        }

        public void Update(TypeWorkDTO item)
        {
            if (item is not null)
            {
                _database.TypeWorks.Update(_mapper.Map<TypeWork>(item));
                _database.Save();
            }
        }
    }
}