using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using System;
using System.Text;

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

        public int? Create(ContractDTO item)
        {
            if (item is not null)
            {
                if (_database.Contracts.GetById(item.Id) is null)
                {
                    var contract = _mapper.Map<Contract>(item);

                    _database.Contracts.Create(contract);
                    _database.Save();
                    return contract.Id;
                }
            }
            return null;
        }

        public void Delete(int id, int? secondId = null)
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

        public ContractDTO GetById(int id   , int? secondId = null)
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

        public bool ExistContractByNumber(string numberContract)
        {
            bool result = false;           

            if (_database.Contracts.Find(x => x.Number == numberContract).FirstOrDefault() is not null)
            {                
                return true;
            }

            var sameContracts = _database.Contracts.GetAll();

            string contractNumberForChecking = TrimWhitespaceIntoNumberOfContract(numberContract);

            foreach (var item in sameContracts)
            {
                if (contractNumberForChecking.Equals(TrimWhitespaceIntoNumberOfContract(item.Number), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return result;
        }

        public List<ContractDTO>? ExistContractAndReturnListSameContracts(string numberContract, DateTime? dateContract)
        {
            List<ContractDTO> contracts = new List<ContractDTO>();

            var contract = _database.Contracts.Find(x => x.Number == numberContract && x.Date == dateContract).FirstOrDefault();

            if (contract is not null)
            {
                contracts.Add(_mapper.Map<ContractDTO>(contract)); 
                return contracts;
            }

            var sameContracts = _database.Contracts.Find(x => x.Date?.ToString("yyyyMMdd") == dateContract?.ToString("yyyyMMdd"));

            string contractNumberForChecking = TrimWhitespaceIntoNumberOfContract(numberContract);

            foreach (var item in sameContracts)
            {
                if (contractNumberForChecking.Equals(TrimWhitespaceIntoNumberOfContract(item.Number), StringComparison.OrdinalIgnoreCase))
                {
                    contracts.Add(_mapper.Map<ContractDTO>(item));
                }
            }

            return contracts;
        }

        private string TrimWhitespaceIntoNumberOfContract(string numberContract)
        {
            if (string.IsNullOrWhiteSpace(numberContract))
            {
                return string.Empty;
            }
            char[] number = numberContract.ToCharArray();
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < number.Length; i++)
            {
                if (!char.IsWhiteSpace(number[i]))
                {
                    stringBuilder.Append(number[i]);
                }
            }

            return stringBuilder.ToString();
        }
    }
}