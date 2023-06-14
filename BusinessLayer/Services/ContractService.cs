using AutoMapper;
using BusinessLayer.Interfaces.Contracts;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;

namespace BusinessLayer.Services
{
    internal class ContractService : IContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public ContractService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public void Create(ContractDTO item)
        {
            if (item is not null)
            {
                if (_database.Contracts.GetById(item.Id) is null)
                {
                    var contract = _mapper.Map<Contract>(item);

                    _database.Contracts.Create(contract);
                    _database.Save();
                }
            }
        }

        public void Delete(int id)
        {
            _database.Contracts.Delete(id);
            _database.Save();
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public IEnumerable<ContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.GetAll());
        }

        public ContractDTO GetById(int id)
        {
            var contract = _database.Contracts.GetById(id);

            if (contract is not null)
            {
                return _mapper.Map<ContractDTO>(contract);
            }
            else
            {
                return null;
            }
        }

        public void Update(ContractDTO item)
        {
            if (item is not null)
            {
                _database.Contracts.Update(_mapper.Map<Contract>(item));
                _database.Save();
            }
        }

        public IEnumerable<ContractDTO> Find(Func<Contract, bool> predicate)
        {
            return _mapper.Map<IEnumerable<ContractDTO>>(_database.Contracts.Find(predicate));
        }
    }
}